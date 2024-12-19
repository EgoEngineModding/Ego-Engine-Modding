using System.Diagnostics;

using EgoEngineLibrary.Formats.TrackQuadTree;
using EgoEngineLibrary.Formats.TrackQuadTree.Static;

namespace Sandbox;

public static class CQuadTreeSandbox
{
    public static void Run(string[] args)
    {
        var (folder, type) = (@"C:\Games\DiRT Demo\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\Grid\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\Dirt 2\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2010\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2011\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2012\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\f12013\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2014\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT 3 Complete Edition\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT Showdown\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\grid 2\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\GRID Autosport\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT Rally\tracks\", CQuadTreeType.Dirt);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2014\tracks\circuits\Abu_Dhabi\route_0", CQuadTreeType.Dirt);
        var typeInfo = CQuadTreeTypeInfo.Get(type);
        var maxLevel = 0;
        var maxTris = 0;
        var maxNodes = 0;
        var maxVerts = 0;
        var maxMats = 0;
        var maxNodeTris = 0;
        var maxNodeVertices = 0;
        var maxNodeMaterials = 0;
        var files = Utils.GetFiles("*.cqtc", folder);
        foreach (var f in files)
        {
            try
            {
                var start = Stopwatch.GetTimestamp();
                var qt = new CQuadTreeFile(File.ReadAllBytes(f), typeInfo)
                {
                    Identifier = Path.GetFileNameWithoutExtension(f)
                };
                
                var info = PrintNodeData(qt);
                maxLevel = Math.Max(maxLevel, info.L);
                maxTris = Math.Max(maxTris, info.T);
                maxVerts = Math.Max(maxVerts, info.V);
                maxMats = Math.Max(maxMats, info.M);
                maxNodes = Math.Max(maxNodes, info.N);
                maxNodeTris = Math.Max(maxNodeTris, info.NT);
                maxNodeVertices = Math.Max(maxNodeVertices, info.NV);
                maxNodeMaterials = Math.Max(maxNodeMaterials, info.NM);
                //continue;
                //var gltf = TrackGroundGltfConverter.Convert(qt);
                //gltf.Save(Path.ChangeExtension(f, ".glb"));
                //var ground2 = GltfTrackGroundConverter.Convert(gltf, CQuadTreeTypeInfo.Get(type));
                var data = new QuadTreeMeshData(typeInfo);
                foreach (var triangle in qt.GetTriangles())
                {
                    data.Add(triangle);
                }

                var cqt = CQuadTree.Create(data.BoundsMin, data.BoundsMax, data);
                var qt2 = cqt.CreateFile();
                PrintNodeData(qt2);
                continue;
                File.WriteAllBytes(Path.Combine(folder, Path.GetFileNameWithoutExtension(f) + "2.cqtc"), qt2.Bytes);

                var gltf2 = TrackGroundGltfConverter.Convert(qt2);
                gltf2.Save(Path.Combine(folder, Path.GetFileNameWithoutExtension(f) + "2.glb"));
                
                Console.WriteLine($"{Stopwatch.GetElapsedTime(start)}");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        var finalInfo = new QuadTreeInfo(maxLevel, maxTris, maxVerts, maxMats, maxNodes, maxNodeTris, maxNodeVertices,
            maxNodeMaterials, 0);
        Console.WriteLine(finalInfo.ToString());
    }

    private static QuadTreeInfo PrintNodeData(CQuadTreeFile qt)
    {
        var maxNodeTris = 0;
        var maxNodeVertices = 0;
        var maxNodeMaterials = 0;
        for (var i = 0; i < qt.NumNodes; ++i)
        {
            var triangles = qt.GetNodeTriangles(i);
            maxNodeTris = Math.Max(maxNodeTris, triangles.Length);
            maxNodeVertices = Math.Max(maxNodeVertices,
                triangles.SelectMany<QuadTreeTriangle, int>(x => [x.A, x.B, x.C]).Distinct().Count());
            maxNodeMaterials = Math.Max(maxNodeMaterials, triangles.Select(x => x.MaterialIndex).Distinct().Count());
        }

        var info = new QuadTreeInfo(qt.GetDepth(), qt.NumTriangles, qt.NumVertices, qt.NumMaterials, qt.NumNodes,
            maxNodeTris, maxNodeVertices, maxNodeMaterials, qt.Bytes.Length);
        Console.WriteLine(info.ToString());
        return info;
    }

    private record QuadTreeInfo(int L, int T, int V, int M, int N, int NT, int NV, int NM, int B)
    {
        public override string ToString()
        {
            return $"L{L} T{T} V{V} M{M} N{N} NT{NT} NV{NV} NM{NM} B{B}";
        }
    }
}
