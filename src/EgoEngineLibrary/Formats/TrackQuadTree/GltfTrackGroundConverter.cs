using System.Collections.Generic;
using System.IO;

using EgoEngineLibrary.Formats.TrackQuadTree.Static;

using SharpGLTF.Runtime;
using SharpGLTF.Schema2;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public static class GltfTrackGroundConverter
{
    public static TrackGround Convert(ModelRoot gltf, VcQuadTreeTypeInfo typeInfo)
    {
        var data = new QuadTreeMeshDataBuilder(typeInfo);
        foreach (var triangle in GetTriangles(gltf))
        {
            data.Add(triangle);
        }

        var ground = TrackGround.Create(data.Build());
        return ground;
    }

    public static CQuadTreeFile Convert(ModelRoot gltf, CQuadTreeTypeInfo typeInfo)
    {
        var data = new QuadTreeMeshDataBuilder(typeInfo);
        foreach (var triangle in GetTriangles(gltf))
        {
            data.Add(triangle);
        }

        var file = CQuadTreeFile.Create(data.Build());
        return file;
    }

    private static IEnumerable<QuadTreeDataTriangle> GetTriangles(ModelRoot gltf)
    {
        var scene = gltf.DefaultScene;
        var sceneTemplate = SceneTemplate.Create(scene, new RuntimeOptions { IsolateMemory = false });
        var sceneInstance = sceneTemplate.CreateInstance();
        sceneInstance.Armature.SetPoseTransforms();

        foreach (var drawableInstance in sceneInstance)
        {
            var gltfMesh = scene.LogicalParent.LogicalMeshes[drawableInstance.Template.LogicalMeshIndex];
            var decoder = gltfMesh.Decode();
            foreach (var p in decoder.Primitives)
            {
                var materialName = p.Material?.Name;
                if (materialName is null || materialName.Length != 4)
                {
                    throw new InvalidDataException(
                        $"Material name must be exactly 4 characters, but was '{materialName}' in node '{drawableInstance.Template.NodeName}'.");
                }

                foreach ((int a, int b, int c) in p.TriangleIndices)
                {
                    var pos0 = p.GetPosition(a, drawableInstance.Transform);
                    var pos1 = p.GetPosition(b, drawableInstance.Transform);
                    var pos2 = p.GetPosition(c, drawableInstance.Transform);
                    var triangle = new QuadTreeDataTriangle(pos0, pos1, pos2, materialName);
                    yield return triangle;
                }
            }
        }
    }
}
