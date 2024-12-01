using System.Collections.Generic;
using System.IO;
using System.Numerics;

using SharpGLTF.Runtime;
using SharpGLTF.Schema2;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public static class GltfTrackGroundConverter
{
    public static TrackGround Convert(ModelRoot gltf, IQuadTreeTypeInfo typeInfo)
    {
        var scene = gltf.DefaultScene;
        var sceneTemplate = SceneTemplate.Create(scene, new RuntimeOptions { IsolateMemory = false });
        var sceneInstance = sceneTemplate.CreateInstance();
        sceneInstance.Armature.SetPoseTransforms();

        var boundsMin = new Vector3(float.MaxValue);
        var boundsMax = new Vector3(float.MinValue);
        var triangles = new List<QuadTreeTriangleData>();
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

                foreach (var (a, b, c) in p.TriangleIndices)
                {
                    var pos0 = p.GetPosition(a, drawableInstance.Transform);
                    var pos1 = p.GetPosition(b, drawableInstance.Transform);
                    var pos2 = p.GetPosition(c, drawableInstance.Transform);
                    var triangle = new QuadTreeTriangleData(pos0, pos1, pos2, materialName);
                    triangles.Add(triangle);

                    var triBounds = triangle.GetBounds();
                    boundsMin = Vector3.Min(boundsMin, triBounds.BoundsMin);
                    boundsMax = Vector3.Max(boundsMax, triBounds.BoundsMax);
                }
            }
        }

        var quadTree = new TrackGroundQuadTree(
            boundsMin - TrackGroundQuadTree.Padding,
            boundsMax + TrackGroundQuadTree.Padding,
            typeInfo);
        foreach (var triangle in triangles)
        {
            quadTree.Add(triangle);
        }

        var ground = TrackGround.Create(quadTree);
        return ground;
    }
}
