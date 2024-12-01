using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public static class TrackGroundGltfConverter
{
    private const string SurfaceName = "trackSurface";
    
    public static ModelRoot Convert(TrackGround ground)
    {
        var sceneBuilder = new SceneBuilder();
        NodeBuilder node = new(SurfaceName);
        var mesh = new MeshBuilder<VertexPosition, VertexEmpty, VertexEmpty>(SurfaceName);

        var materialMap = new Dictionary<string, MaterialBuilder>();
        foreach (var data in ground.TraverseGrid())
        {
            ConvertQuadTree(data.QuadTree, mesh, materialMap);
        }
        
        sceneBuilder.AddRigidMesh(mesh, node);
        return sceneBuilder.ToGltf2();
    }

    public static ModelRoot Convert(VcQuadTreeFile quadTree)
    {
        var sceneBuilder = new SceneBuilder();
        NodeBuilder node = new(SurfaceName);
        var mesh = new MeshBuilder<VertexPosition, VertexEmpty, VertexEmpty>(SurfaceName);

        var materialMap = new Dictionary<string, MaterialBuilder>();
        ConvertQuadTree(quadTree, mesh, materialMap);

        sceneBuilder.AddRigidMesh(mesh, node);
        return sceneBuilder.ToGltf2();
    }

    public static ModelRoot Convert(VcQuadTreeFile quadTree, int nodeIndex)
    {
        var sceneBuilder = new SceneBuilder();
        NodeBuilder node = new(SurfaceName);
        var mesh = new MeshBuilder<VertexPosition, VertexEmpty, VertexEmpty>(SurfaceName);

        var materialMap = new Dictionary<string, MaterialBuilder>();
        var count = quadTree.GetNodeTriangles(nodeIndex, []);
        var indices = new int[count];
        quadTree.GetNodeTriangles(nodeIndex, indices);
        var triangles = quadTree.GetTriangles();
        ConvertTriangles(indices.Select(i => triangles[i]), mesh, materialMap);

        sceneBuilder.AddRigidMesh(mesh, node);
        return sceneBuilder.ToGltf2();
    }

    private static void ConvertQuadTree(VcQuadTreeFile quadTree, IMeshBuilder<MaterialBuilder> mesh,
        Dictionary<string, MaterialBuilder> materialMap)
    {
        ConvertTriangles(quadTree.GetTriangles(), mesh, materialMap);
    }

    private static void ConvertTriangles(IEnumerable<QuadTreeTriangleData> triangles, IMeshBuilder<MaterialBuilder> mesh,
        Dictionary<string, MaterialBuilder> materialMap)
    {
        foreach (var triangle in triangles)
        {
            if (!materialMap.TryGetValue(triangle.Material, out var material))
            {
                material = CreateMaterial(triangle.Material);
                materialMap.Add(triangle.Material, material);
            }
            
            var pb = mesh.UsePrimitive(material);
            pb.AddTriangle(
                CreateVertexBuilder(triangle.Position0),
                CreateVertexBuilder(triangle.Position1),
                CreateVertexBuilder(triangle.Position2));
        }

        return;

        static IVertexBuilder CreateVertexBuilder(Vector3 position)
        {
            var vb = new VertexBuilder<VertexPosition, VertexEmpty, VertexEmpty>();
            vb.Geometry.Position = position;
            return vb;
        }
        static MaterialBuilder CreateMaterial(string name)
        {
            var mat = new MaterialBuilder(name);
            mat.WithMetallicRoughnessShader()
                .WithMetallicRoughness(0.1f, 0.75f)
                .WithBaseColor(new Vector4(1, 1, 1, 1));
            return mat;
        }
    }
}
