using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

using EgoEngineLibrary.Archive.Jpk;
using EgoEngineLibrary.Formats.TrackQuadTree;
using EgoEngineLibrary.Formats.TrackQuadTree.Static;

namespace Sandbox;

public static class VcQuadTreeSandbox
{
    public static void Run(string[] args)
    {
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\Grid\tracks\", VcQuadTreeType.RaceDriverGrid);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\Dirt 2\tracks\", VcQuadTreeType.RaceDriverGrid);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2014\tracks\", VcQuadTreeType.RaceDriverGrid);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT 3 Complete Edition\tracks\", VcQuadTreeType.Dirt3);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT Showdown\tracks\", VcQuadTreeType.DirtShowdown);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\grid 2\tracks\", VcQuadTreeType.DirtShowdown);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\GRID Autosport\tracks\", VcQuadTreeType.DirtShowdown);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT Rally\tracks\", VcQuadTreeType.DirtShowdown);
        var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2014\tracks\circuits\Abu_Dhabi\route_0", VcQuadTreeType.RaceDriverGrid);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT Showdown\tracks\locations\japan\yokohama_docks\route_0", VcQuadTreeType.DirtShowdown);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT Showdown\tracks\locations\japan\shibuya\route_0", VcQuadTreeType.DirtShowdown);
        var typeInfo = VcQuadTreeTypeInfo.Get(type);
        var maxEntries = 0;
        var maxTris = 0;
        var maxNodes = 0;
        var maxVerts = 0;
        var maxMats = 0;
        var maxNodeTris = 0;
        var maxLevel = 0;
        var files = Utils.GetFiles("track*.jpk", folder);
        foreach (var f in files)
        {
            try
            {
                var start = Stopwatch.GetTimestamp();
                using var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                var jpk = new JpkFile();
                jpk.Read(fs);
                
                //ExamineNode(jpk.Entries, type);
                var info = PrintNodeData(jpk.Entries, typeInfo);
                maxEntries = Math.Max(maxEntries, info.E);
                maxLevel = Math.Max(maxLevel, info.L);
                maxTris = Math.Max(maxTris, info.T);
                maxVerts = Math.Max(maxVerts, info.V);
                maxMats = Math.Max(maxMats, info.M);
                maxNodes = Math.Max(maxNodes, info.N);
                maxNodeTris = Math.Max(maxNodeTris, info.NT);
                //PrintSubDivs(jpk.Entries);
                //continue;
                var ground = TrackGround.Load(jpk, typeInfo);
                //var gltf = TrackGroundGltfConverter.Convert(ground, true);
                //gltf.Save(Path.ChangeExtension(f, ".glb"));
                //var ground2 = GltfTrackGroundConverter.Convert(gltf, VcQuadTreeTypeInfo.Get(type));
                var boundsMin = new Vector3(float.MaxValue);
                var boundsMax = new Vector3(float.MinValue);
                var triangles = new List<QuadTreeDataTriangle>();
                foreach (var node in ground.TraverseGrid())
                {
                    foreach (var triangle in node.QuadTree.GetTriangles())
                    {
                        triangles.Add(triangle);
                
                        var triBounds = triangle.GetBounds();
                        boundsMin = Vector3.Min(boundsMin, triBounds.BoundsMin);
                        boundsMax = Vector3.Max(boundsMax, triBounds.BoundsMax);
                    }
                }
                var gqt2 = TrackGroundQuadTree.Create(boundsMin, boundsMax, typeInfo);
                foreach (var triangle in triangles)
                {
                    gqt2.Add(triangle);
                }
                var ground2 = TrackGround.Create(gqt2);
                
                var jpk2 = ground2.Save();
                PrintNodeData(jpk2.Entries, typeInfo);
                continue;
                using var fs3 = File.Open(Path.Combine(folder, "track2.jpk"), FileMode.Create, FileAccess.Write, FileShare.Read);
                jpk2.Write(fs3);

                var ground3 = TrackGround.Load(jpk2, typeInfo);
                var gltf3 = TrackGroundGltfConverter.Convert(ground3, true);
                gltf3.Save(Path.Combine(folder, "track2.glb"));
                
                Console.WriteLine($"{Stopwatch.GetElapsedTime(start)}");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        var finalInfo = new TrackJpkInfo(maxEntries, maxLevel, maxTris, maxVerts, maxMats, maxNodes, maxNodeTris);
        Console.WriteLine(finalInfo.ToString());
    }

    private static void ConvertShowdownToDirt3(string f, JpkFile jpk, VcQuadTreeTypeInfo typeInfo)
    {
        var materials1 = new HashSet<string>();
        var materials2 = new HashSet<string>();
        foreach (var entry in jpk.Entries)
        {
            if (!entry.Name.StartsWith("qt_"))
            {
                continue;
            }

            var vcqtc = new VcQuadTreeFile(entry.Data, typeInfo);
            vcqtc.GetMaterials(materials1);
            vcqtc.ConvertType(VcQuadTreeType.Dirt3);
            entry.Data = vcqtc.Bytes;
            vcqtc.GetMaterials(materials2);
        }
                
        using var fs2 = new FileStream(Path.ChangeExtension(f, ".new.jpk"), FileMode.Create, FileAccess.Write, FileShare.Read);
        jpk.Write(fs2);
    }
    
    private static void ExamineNode(IReadOnlyList<JpkEntry> entries, VcQuadTreeTypeInfo typeInfo)
    {
        foreach (var entry in entries)
        {
            if (!entry.Name.EndsWith(".vcqtc"))
            {
                continue;
            }
                    
            var vcqtc = new VcQuadTreeFile(entry.Data, typeInfo);
            var tris = vcqtc.GetTriangles();
            var data = new QuadTreeMeshData(typeInfo);
            for (var i = 0; i < vcqtc.NumTriangles; ++i)
            {
                var tri = tris[i];
                data.Add(tri);
            }

            data.Optimize();
            var quadTree = VcQuadTree.Create(data.BoundsMin, data.BoundsMax, data);
            //var quadTree = VcQuadTree.Create(vcqtc.Header.BoundMin, vcqtc.Header.BoundMax, dat);
            var qtc = VcQuadTreeFile.Create(quadTree);
            File.WriteAllBytes(@"C:\Games\Steam\steamapps\common\F1 2014\tracks\circuits\Abu_Dhabi\route_0\track2\qtc.vcqtc", qtc.Bytes);
        }
    }

    private static TrackJpkInfo PrintNodeData(IReadOnlyList<JpkEntry> entries, VcQuadTreeTypeInfo typeInfo)
    {
        var maxTris = 0;
        var maxNodes = 0;
        var maxVerts = 0;
        var maxMats = 0;
        var maxNodeTris = 0;
        var maxLevel = 0;
        foreach (var entry in entries)
        {
            if (!entry.Name.EndsWith(".vcqtc"))
            {
                continue;
            }
                    
            var vcqtc = new VcQuadTreeFile(entry.Data, typeInfo);
            var count = 0;
            for (var i = 0; i < vcqtc.NumNodes; ++i)
            {
                var tCount = vcqtc.GetNodeTriangles(i, []);
                count = Math.Max(count, tCount);
            }

            maxNodes = Math.Max(maxNodes, vcqtc.NumNodes);
            maxTris = Math.Max(maxTris, vcqtc.NumTriangles);
            maxVerts = Math.Max(maxVerts, vcqtc.NumVertices);
            maxMats = Math.Max(maxMats, vcqtc.NumMaterials);
            maxNodeTris = Math.Max(maxNodeTris, count);
            maxLevel = Math.Max(maxLevel, entry.Name.AsSpan().Count('_'));
        }

        var info = new TrackJpkInfo(entries.Count, maxLevel, maxTris, maxVerts, maxMats, maxNodes, maxNodeTris);
        Console.WriteLine(info.ToString());
        return info;
    }
    
    private static void PrintBounds(IReadOnlyList<JpkEntry> entries, VcQuadTreeTypeInfo typeInfo)
    {
        var children = new List<QuadTreeNodeInfo>();
        foreach (var entry in entries)
        {
            if (!entry.Name.StartsWith("qt_"))
            {
                continue;
            }

            var vcqtc = new VcQuadTreeFile(entry.Data, typeInfo);
            children.Add(new QuadTreeNodeInfo(entry.Name, 0, vcqtc.BoundsMinXz, vcqtc.BoundsMaxXz));
        }

        foreach (var child in children
                     .OrderBy(x => (x.Max - x.Min).Length())
                     .ThenBy(x => x.Min.Y)
                     .ThenBy(x => x.Min.X))
        {
            Console.WriteLine($"{child.Name} {child.Min.X} {child.Min.Y} {child.Max.X} {child.Max.Y}");
        }
    }

    private static void PrintSubDivs(IReadOnlyList<JpkEntry> entries)
    {
        var children = new List<QuadTreeNodeInfo>();
        var info = entries.FirstOrDefault(x => x.Name.Equals("qt.info"));
        Vector2 boundsMin = Vector2.Zero, boundsMax = Vector2.Zero;
        bool boundsValid = false;
        if (info is not null)
        {
            boundsValid = true;
            var floatData = MemoryMarshal.Cast<byte, float>(info.Data.AsSpan(0, 24));
            boundsMin = new Vector2(floatData[0], floatData[2]);
            boundsMax = new Vector2(floatData[3], floatData[5]);
        }

        foreach (var entry in entries)
        {
            var floatData = MemoryMarshal.Cast<byte, float>(entry.Data.AsSpan(0, 24));
            if (!boundsValid)
            {
                boundsMin = new Vector2(floatData[0], floatData[2]);
                boundsMax = new Vector2(floatData[3], floatData[5]);
            }

            var topLevelCell = 0x8000;
            var zAdjust = 0x8000;
            var xPos = 0;
            var zPos = 0;
            var level = 0;
            var name = entry.Name.AsSpan(2);
            while (name.Length > 0 && name[0] == '_')
            {
                if (name[1] == '1')
                {
                    xPos += topLevelCell;
                }

                if (name[2] == '1')
                {
                    zPos += zAdjust;
                }

                level += 1;
                topLevelCell /= 2;
                zAdjust /= 2;
                name = name[3..];
            }

            var width = 0x10000 >> level;
            var subMin = new Vector2(xPos, zPos);
            var subMax = new Vector2(xPos + width, zPos + width);
            var scale = (boundsMax - boundsMin) * 0.000015258789f;
            var axisBoundsMin = (subMin * scale) + boundsMin;
            var axisBoundsMax = (subMax * scale) + boundsMin;

            if (floatData[0] < axisBoundsMin.X || floatData[2] < axisBoundsMin.Y ||
                floatData[3] > axisBoundsMax.X || floatData[5] > axisBoundsMax.Y)
            {
                //Debugger.Break();
            }

            topLevelCell = (zPos / 2048) * 32 + (xPos / 2048);
            //children.Add(new QuadTreeNodeInfo(entry.Name, topLevelCell, subMin, subMax - Vector2.One));
            children.Add(new QuadTreeNodeInfo(entry.Name, topLevelCell, axisBoundsMin, axisBoundsMax));
        }

        foreach (var child in children)
        {
            Console.WriteLine($"{child.Name} {child.Select} {child.Min.X} {child.Min.Y} {child.Max.X} {child.Max.Y}");
        }
    }

    private static void DumpObj(JpkFile jpk, string jpkFolder, VcQuadTreeTypeInfo typeInfo)
    {
        var outputFolder = Path.Combine(jpkFolder, "trackObj");
        Directory.CreateDirectory(outputFolder);
        foreach (var entry in jpk.Entries)
        {
            if (!entry.Name.StartsWith("qt_"))
            {
                continue;
            }
                    
            var vcqtc = new VcQuadTreeFile(entry.Data, typeInfo);
            using var fs = File.Open(Path.Combine(outputFolder, entry.Name + ".obj"), FileMode.Create,
                FileAccess.Write, FileShare.Read);
            using var sw = new StreamWriter(fs);
            vcqtc.DumpObj(sw);
        }
    }

    private record QuadTreeNodeInfo(string Name, int Select, Vector2 Min, Vector2 Max);
    private record TrackJpkInfo(int E, int L, int T, int V, int M, int N, int NT)
    {
        public override string ToString()
        {
            return $"E{E} L{L} T{T} V{V} M{M} N{N} NT{NT}";
        }
    }
}
