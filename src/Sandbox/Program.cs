using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

using EgoEngineLibrary.Archive.Jpk;
using EgoEngineLibrary.Formats.TrackQuadTree;
using EgoEngineLibrary.Xml;

namespace Sandbox;

internal class Program
{
    static void Main(string[] args)
    {
        VcqtcSandbox();
    }

    private static void XmlSandbox()
    {
        var files = GetFiles(@"C:\Games\Steam\steamapps\common\F1 2012");
        foreach (var f in files)
        {
            try
            {
                using var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                var isXmlFile = XmlFile.IsXmlFile(fs);
                // if (f.EndsWith("reflections.xml"))
                // {
                //     int a = 55;
                // }
                Console.WriteLine($"{f} {isXmlFile}");
                var xml = new XmlFile(fs);
                //Console.WriteLine(xml.type);

                if (xml.Type != XmlType.Text)
                {
                    using var ms = new MemoryStream();
                    xml.Write(ms);

                    var mso = new MemoryStream();
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.CopyTo(mso);
                    mso.Seek(0, SeekOrigin.Begin);
                    ms.Seek(0, SeekOrigin.Begin);
                    var orig = mso.ToArray();
                    var writ = ms.ToArray();
                    if (!orig.SequenceEqual(writ))
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
            catch (Exception e)
            {
                using var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                var type = XmlFile.GetXmlType(fs);
                if (type != XmlType.Text)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        static IEnumerable<string> GetFiles(params string[] gameFolders)
        {
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".xml" };
            var files = Enumerable.Empty<string>();
            foreach (var folder in gameFolders)
            {
                var folderFiles = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(x => extensions.Contains(Path.GetExtension(x)));
                files = files.Concat(folderFiles);
            }

            return files;
        }
    }

    private static void VcqtcSandbox()
    {
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\Grid\tracks\", VcQuadTreeType.RaceDriverGrid);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\Dirt 2\tracks\", VcQuadTreeType.RaceDriverGrid);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2014\tracks\", VcQuadTreeType.RaceDriverGrid);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT 3 Complete Edition\tracks\", VcQuadTreeType.Dirt3);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\GRID Autosport\tracks\", VcQuadTreeType.GridAutosport);
        var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2014\tracks\circuits\Abu_Dhabi\route_0", VcQuadTreeType.RaceDriverGrid);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT Showdown\tracks\locations\japan\yokohama_docks\route_0", VcQuadTreeType.GridAutosport);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT Showdown\tracks\locations\japan\shibuya\route_0", VcQuadTreeType.GridAutosport);
        var typeInfo = VcQuadTreeTypeInfo.Get(type);
        var files = GetFiles("track*.jpk", folder);
        foreach (var f in files)
        {
            try
            {
                using var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                var jpk = new JpkFile();
                jpk.Read(fs);
                
                //PrintNodeData(jpk.Entries, type);
                PrintSubDivs(jpk.Entries);
                continue;
                var ground = TrackGround.Load(jpk, type);
                var gltf = TrackGroundGltfConverter.Convert(ground);
                //gltf.Save(Path.ChangeExtension(f, ".glb"));
                //var ground2 = GltfTrackGroundConverter.Convert(gltf, VcQuadTreeTypeInfo.Get(type));
                var boundsMin = new Vector3(float.MaxValue);
                var boundsMax = new Vector3(float.MinValue);
                var triangles = new List<QuadTreeTriangleData>();
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
                var gqt2 = new TrackGroundQuadTree(boundsMin, boundsMax, typeInfo);
                foreach (var triangle in triangles)
                {
                    gqt2.Add(triangle);
                }

                var ground2 = TrackGround.Create(gqt2);
                
                var jpk2 = ground2.Save();
                using var fs3 = File.Open(Path.Combine(folder, "track2.jpk"), FileMode.Create, FileAccess.Write, FileShare.Read);
                jpk2.Write(fs3);
                var qtc = new VcQuadTreeFile(jpk2.Entries[0].Data, type);
                var qtcGltf = TrackGroundGltfConverter.Convert(qtc);
                qtcGltf.Save(@"C:\Games\Steam\steamapps\common\F1 2014\tracks\circuits\Abu_Dhabi\route_0\track2\qtc.glb");

                var ground3 = TrackGround.Load(jpk2, type);
                var gltf3 = TrackGroundGltfConverter.Convert(ground3);
                gltf3.Save(Path.Combine(folder, "track2.glb"));
                
                return;
                
                //PrintNodeData(jpk.Entries, type);
                //PrintBounds(jpk.Entries, type);
                PrintSubDivs(jpk.Entries);
                var objOutputFolder = Path.Combine(folder, "trackObj");
                Directory.CreateDirectory(objOutputFolder);
                DumpObj(jpk.Entries, objOutputFolder, type);
                return;

                var materials1 = new HashSet<string>();
                var materials2 = new HashSet<string>();
                foreach (var entry in jpk.Entries)
                {
                    if (!entry.Name.StartsWith("qt_"))
                    {
                        continue;
                    }
                    
                    var vcqtc = new VcQuadTreeFile(entry.Data, VcQuadTreeType.GridAutosport);
                    vcqtc.GetMaterials(materials1);
                    vcqtc.ConvertType(VcQuadTreeType.Dirt3);
                    //var str1 = Convert.ToHexString(entry.Data);
                    var str2 = Convert.ToHexString(vcqtc.Bytes);
                    entry.Data = vcqtc.Bytes;
                    vcqtc.GetMaterials(materials2);
                }
                
                using var fs2 = new FileStream(Path.ChangeExtension(f, ".new.jpk"), FileMode.Create, FileAccess.Write, FileShare.Read);
                jpk.Write(fs2);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void PrintNodeData2(IReadOnlyList<JpkEntry> entries, VcQuadTreeType type)
        {
            foreach (var entry in entries)
            {
                if (!entry.Name.StartsWith("qt_"))
                {
                    continue;
                }
                    
                var vcqtc = new VcQuadTreeFile(entry.Data, type);
                var unrawTri = vcqtc.GetTriangles();//.OrderBy(x => x.Material).ToArray();
                //     .ThenBy(x =>
                //     {
                //         var bounds = x.GetBounds();
                //         return (bounds.BoundsMax - bounds.BoundsMin).Length();
                //     }).ToArray();
                //var unrawTri = vcqtc.GetTriangles();
                var dat = new QuadTreeMeshData(VcQuadTreeTypeInfo.Get(type));
                for (var i = 0; i < vcqtc.NumTriangles; ++i)
                {
                    var tri = unrawTri[i];
                    dat.Add(tri);
                }

                var bad = new List<(int, QuadTreeTriangle)>();
                for (var i = 0; i < dat.Triangles.Count; ++i)
                {
                    var tri = dat.Triangles[i];
                    var offset1 = Math.Abs(tri.B - tri.A);
                    var offset2 = Math.Abs(tri.C - tri.A);
                    if (offset1 > 255 || offset2 > 255)
                    {
                        bad.Add((i, tri));
                    }
                }
                
                var quadTree = VcQuadTree.Create(dat.BoundsMin, dat.BoundsMax, dat);
                quadTree.Optimize();
                var qtc = VcQuadTreeFile.Create(quadTree);
                File.WriteAllBytes(@"C:\Games\Steam\steamapps\common\F1 2014\tracks\circuits\Abu_Dhabi\route_0\track2\qtc.vcqtc", qtc.Bytes);
                var qtcGltf = TrackGroundGltfConverter.Convert(qtc);
                qtcGltf.Save(@"C:\Games\Steam\steamapps\common\F1 2014\tracks\circuits\Abu_Dhabi\route_0\track2\qtc.glb");
                var vcqtcGltf7 = TrackGroundGltfConverter.Convert(vcqtc, 7);
                vcqtcGltf7.Save(@"C:\Games\Steam\steamapps\common\F1 2014\tracks\circuits\Abu_Dhabi\route_0\track2\qtc7o.glb");
                var qtcGltf7 = TrackGroundGltfConverter.Convert(qtc, 7);
                qtcGltf7.Save(@"C:\Games\Steam\steamapps\common\F1 2014\tracks\circuits\Abu_Dhabi\route_0\track2\qtc7.glb");
                var hs = new HashSet<int>();
                for (var i = 0; i < qtc.NumNodes; ++i)
                {
                    var count = qtc.GetNodeTriangles(i, []);
                    var indices = new int[count];
                    qtc.GetNodeTriangles(i, indices);
                    for (var j = 0; j < count; ++j)
                    {
                        hs.Add(indices[j]);
                    }
                }

                var unused = Enumerable.Range(0, dat.Triangles.Count).Except(hs).ToArray();

                var verts = new List<Vector3>();
                var vertMap = new Dictionary<int, List<int>>();
                var vertDists = new List<double>();
                for (var i = 0; i < vcqtc.NumVertices; ++i)
                {
                    var vert = vcqtc.GetRawVertex(i);
                    var ind = verts.IndexOf(vert);
                    verts.Add(vert);
                    vertDists.Add(Math.Sqrt(vert.X * vert.X + vert.Z * vert.Z));

                    if (ind == -1)
                    {
                        continue;
                    }
                    
                    if (!vertMap.TryGetValue(ind, out var indices))
                    {
                        indices = new List<int>();
                        vertMap[ind] = indices;
                    }
                    
                    indices.Add(i);
                }

                foreach (var kvp in vertMap)
                {
                    if (vertMap.Values.Count == 0)
                    {
                        continue;
                    }

                    Console.WriteLine($"{kvp.Key} {string.Join(',', kvp.Value)}");
                }

                var tris = new List<QuadTreeTriangle>();
                var triDists = new List<double>();
                var triBoundDists = new List<double>();
                for (var i = 0; i < dat.Triangles.Count; ++i)
                {
                    // var rawTri = vcqtc.GetTriangle(i);
                    // var tri = new QuadTreeTriangle(rawTri.Vertex0, rawTri.Vertex1, rawTri.Vertex2, rawTri.MaterialIndex);
                    var tri = dat.Triangles[i];
                    if (tri.MaterialIndex == 4) tris.Add(tri);
                    var offset1 = tri.B - tri.A;
                    var offset2 = tri.C - tri.A;
                    triDists.Add(Math.Max(offset1, offset2));

                    var unrawTriBounds = unrawTri[i].GetBounds();
                    var minBounds = new Vector2(unrawTriBounds.BoundsMin.X, unrawTriBounds.BoundsMin.Z);
                    var maxBounds = new Vector2(unrawTriBounds.BoundsMax.X, unrawTriBounds.BoundsMax.Z);
                    triBoundDists.Add((maxBounds - minBounds).Length());
                }

                var maxDist = triDists.Max();
                var maxDist2 = triBoundDists.Max();
            }
        }
        static void PrintNodeData(IReadOnlyList<JpkEntry> entries, VcQuadTreeType type)
        {
            var maxTris = 0;
            var maxNodes = 0;
            var maxVerts = 0;
            var maxMats = 0;
            var maxNodeTris = 0;
            foreach (var entry in entries)
            {
                if (!entry.Name.StartsWith("qt_"))
                {
                    continue;
                }
                    
                var vcqtc = new VcQuadTreeFile(entry.Data, type);
                var count = 0;
                var index = 0;
                for (var i = 0; i < vcqtc.NumNodes; ++i)
                {
                    var tCount = vcqtc.GetNodeTriangles(i, []);
                    count = Math.Max(count, tCount);
                    if (count == tCount)
                    {
                        index = i;
                    }

                    Span<int> indices = new int[tCount];
                    vcqtc.GetNodeTriangles(i, indices);

                    var matIndices = new HashSet<int>();
                    for (var j = 0; j < indices.Length; ++j)
                    {
                        matIndices.Add(vcqtc.GetTriangle(indices[j]).MaterialIndex);
                    }
                    
                    //Console.WriteLine($"{tCount} {matIndices.Count}");
                }

                maxNodes = Math.Max(maxNodes, vcqtc.NumNodes);
                maxTris = Math.Max(maxTris, vcqtc.NumTriangles);
                maxVerts = Math.Max(maxVerts, vcqtc.NumVertices);
                maxMats = Math.Max(maxMats, vcqtc.NumMaterials);
                maxNodeTris = Math.Max(maxNodeTris, count);
                
                //Console.WriteLine($"{vcqtc.NumTriangles} {vcqtc.NumVertices} {vcqtc.NumNodes} {vcqtc.NumMaterials} {index} {count}");
                //var children = new List<QuadTreeNodeInfo>();
                // for (var i = 0; i < 4; ++i)
                // {
                //     var (min, max) = (vcqtc.BoundsMinXz, vcqtc.BoundsMaxXz);
                //     vcqtc.GetNodeChild(0, i, ref min, ref max);
                //     children.Add(new QuadTreeNodeInfo(entry.Name, i, min, max));
                // }
                //
                // foreach (var child in children)
                // {
                //     Console.WriteLine($"{child.Name} {child.Select} {child.Min.X} {child.Min.Y} {child.Max.X} {child.Max.Y}");
                // }
            }
            
            Console.WriteLine($"{maxTris} {maxVerts} {maxNodes} {maxMats} {maxNodeTris}");
        }
        static void PrintBounds(IReadOnlyList<JpkEntry> entries, VcQuadTreeType type)
        {
            var children = new List<QuadTreeNodeInfo>();
            foreach (var entry in entries)
            {
                if (!entry.Name.StartsWith("qt_"))
                {
                    continue;
                }

                var vcqtc = new VcQuadTreeFile(entry.Data, type);
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
        static void PrintSubDivs(IReadOnlyList<JpkEntry> entries)
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
        static void DumpObj(IReadOnlyList<JpkEntry> entries, string outputFolder, VcQuadTreeType type)
        {
            foreach (var entry in entries)
            {
                if (!entry.Name.StartsWith("qt_"))
                {
                    continue;
                }
                    
                var vcqtc = new VcQuadTreeFile(entry.Data, type);
                using var fs = File.Open(Path.Combine(outputFolder, entry.Name + ".obj"), FileMode.Create,
                    FileAccess.Write, FileShare.Read);
                using var sw = new StreamWriter(fs);
                vcqtc.DumpObj(sw);
            }
        }
    }

    private record QuadTreeNodeInfo(string Name, int Select, Vector2 Min, Vector2 Max);
        
    private static IEnumerable<string> GetFiles(string filter, params string[] gameFolders)
    {
        var files = Enumerable.Empty<string>();
        foreach (var folder in gameFolders)
        {
            var folderFiles = Directory.EnumerateFiles(folder, filter, SearchOption.AllDirectories);
            files = files.Concat(folderFiles);
        }

        return files;
    }
}
