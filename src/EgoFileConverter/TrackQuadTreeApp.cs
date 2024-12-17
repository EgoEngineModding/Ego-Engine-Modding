using System;
using System.IO;

using ConsoleAppFramework;

using EgoEngineLibrary.Archive.Jpk;
using EgoEngineLibrary.Formats.TrackQuadTree;

using Microsoft.Extensions.Logging;

using SharpGLTF.Schema2;

namespace EgoFileConverter;

internal class TrackQuadTreeApp(ILogger<TrackQuadTreeApp> logger)
{
    /// <summary>Convert track quad tree file (track.jpk and vcqtc) to glTF.</summary>
    /// <param name="filePath">The track quad tree file path.</param>
    /// <param name="outputFilePath">-o, The output file path.</param>
    public void VcToGltf([Argument] string filePath, string? outputFilePath = null)
    {
        if (filePath.EndsWith("track.jpk", StringComparison.InvariantCultureIgnoreCase))
        {
            logger.LogInformation("Converting track.jpk to glTF: {FileName}", Path.GetFileName(filePath));
            using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var jpk = new JpkFile();
            jpk.Read(fs);

            var typeInfo = TrackGround.Identify(jpk);
            logger.LogInformation("Quad tree type is {TypeInfo}", typeInfo);
                    
            var ground = TrackGround.Load(jpk, typeInfo);
            var gltf = TrackGroundGltfConverter.Convert(ground);
            outputFilePath ??= filePath + ".glb";
            gltf.Save(outputFilePath);
            logger.LogInformation("Success, track.jpk converted: {FileName}", Path.GetFileName(outputFilePath));
        }
        else if (filePath.EndsWith(".vcqtc", StringComparison.InvariantCultureIgnoreCase))
        {
            logger.LogInformation("Converting vcqtc to glTF: {FileName}", Path.GetFileName(filePath));
            var data = File.ReadAllBytes(filePath);
            var quadTree = new VcQuadTreeFile(data);
                    
            var gltf = TrackGroundGltfConverter.Convert(quadTree);
            outputFilePath ??= filePath + ".glb";
            gltf.Save(outputFilePath);
            logger.LogInformation("Success, vcqtc converted: {FileName}", Path.GetFileName(outputFilePath));
        }
        else
        {
            throw new NotSupportedException($"File '{filePath}' is not recognized.");
        }
    }

    /// <summary>Convert glTF file to track quad tree (track.jpk).</summary>
    /// <param name="filePath">The glTF file path.</param>
    /// <param name="type">-t, The type of quad tree.</param>
    /// <param name="outputFilePath">-o, The output file path.</param>
    public void GltfToVc([Argument] string filePath, VcQuadTreeType type, string? outputFilePath = null)
    {
        logger.LogInformation("Converting glTF to track.jpk: {FileName}", Path.GetFileName(filePath));
        var gltf = ModelRoot.Load(filePath);
        var ground = GltfTrackGroundConverter.Convert(gltf, VcQuadTreeTypeInfo.Get(type));
        var jpk = ground.Save();
        
        outputFilePath ??= filePath + ".track.jpk";
        using var fs = File.Open(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        jpk.Write(fs);
        logger.LogInformation("Success, glTF converted: {FileName}", Path.GetFileName(outputFilePath));
    }
}
