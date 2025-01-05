using System;
using System.IO;

using ConsoleAppFramework;

using EgoEngineLibrary.Archive.Jpk;
using EgoEngineLibrary.Formats.TrackQuadTree;
using EgoEngineLibrary.Formats.TrackQuadTree.Static;

using Microsoft.Extensions.Logging;

using SharpGLTF.Schema2;

namespace EgoFileConverter;

internal class TrackQuadTreeApp(ILogger<TrackQuadTreeApp> logger)
{
    private const string TrackJpkExtension = ".track.jpk";
    private const string VcqtcExtension = ".vcqtc";
    private const string GlbExtension = ".glb";

    /// <summary>Convert track quad tree file (track.jpk and vcqtc) to a different type.</summary>
    /// <param name="filePath">The track quad tree file path.</param>
    /// <param name="targetType">-tt, The target type of quad tree.</param>
    /// <param name="outputFilePath">-o, The output file path.</param>
    public void VcConvertType([Argument] string filePath, VcQuadTreeType targetType, string? outputFilePath = null)
    {
        if (filePath.EndsWith(VcqtcExtension, StringComparison.InvariantCultureIgnoreCase))
        {
            logger.LogInformation("Converting vcqtc to type {Type}: {FileName}", targetType, Path.GetFileName(filePath));
            var data = File.ReadAllBytes(filePath);

            var typeInfo = VcQuadTreeFile.Identify(data);
            logger.LogInformation("Quad tree type is {TypeInfo}", typeInfo);

            var quadTree = new VcQuadTreeFile(data, typeInfo);
            var qt2 = quadTree.ConvertType(VcQuadTreeTypeInfo.Get(targetType));
            outputFilePath ??= filePath + VcqtcExtension;
            File.WriteAllBytes(outputFilePath, qt2.Bytes);
            logger.LogInformation("Success, vcqtc converted: {FileName}", Path.GetFileName(outputFilePath));
        }
        else
        {
            logger.LogInformation("Converting track.jpk to type {Type}: {FileName}", targetType, Path.GetFileName(filePath));
            using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var jpk = new JpkFile();
            jpk.Read(fs);

            var typeInfo = TrackGround.Identify(jpk);
            logger.LogInformation("Quad tree type is {TypeInfo}", typeInfo);

            var targetTypeInfo = VcQuadTreeTypeInfo.Get(targetType);
            foreach (var entry in jpk.Entries)
            {
                if (!entry.Name.EndsWith(VcqtcExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var qt = new VcQuadTreeFile(entry.Data);
                var qt2 = qt.ConvertType(targetTypeInfo);
                entry.Data = qt2.Bytes;
            }

            outputFilePath ??= filePath + TrackJpkExtension;
            using var fso = File.Open(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            jpk.Write(fso);
            logger.LogInformation("Success, track.jpk converted: {FileName}", Path.GetFileName(outputFilePath));
        }
    }

    /// <summary>Convert track quad tree file (track.jpk and vcqtc) to glTF.</summary>
    /// <param name="filePath">The track quad tree file path.</param>
    /// <param name="outputFilePath">-o, The output file path.</param>
    public void VcToGltf([Argument] string filePath, string? outputFilePath = null)
    {
        if (filePath.EndsWith(VcqtcExtension, StringComparison.InvariantCultureIgnoreCase))
        {
            logger.LogInformation("Converting vcqtc to glTF: {FileName}", Path.GetFileName(filePath));
            var data = File.ReadAllBytes(filePath);

            var typeInfo = VcQuadTreeFile.Identify(data);
            logger.LogInformation("Quad tree type is {TypeInfo}", typeInfo);

            var quadTree = new VcQuadTreeFile(data, typeInfo);
            var gltf = TrackGroundGltfConverter.Convert(quadTree);
            outputFilePath ??= filePath + GlbExtension;
            gltf.Save(outputFilePath);
            logger.LogInformation("Success, vcqtc converted: {FileName}", Path.GetFileName(outputFilePath));
        }
        else
        {
            logger.LogInformation("Converting track.jpk to glTF: {FileName}", Path.GetFileName(filePath));
            using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var jpk = new JpkFile();
            jpk.Read(fs);

            var typeInfo = TrackGround.Identify(jpk);
            logger.LogInformation("Quad tree type is {TypeInfo}", typeInfo);
                    
            var ground = TrackGround.Load(jpk, typeInfo);
            var gltf = TrackGroundGltfConverter.Convert(ground);
            outputFilePath ??= filePath + GlbExtension;
            gltf.Save(outputFilePath);
            logger.LogInformation("Success, track.jpk converted: {FileName}", Path.GetFileName(outputFilePath));
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
        
        outputFilePath ??= filePath + TrackJpkExtension;
        using var fs = File.Open(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        jpk.Write(fs);
        logger.LogInformation("Success, glTF converted: {FileName}", Path.GetFileName(outputFilePath));
    }

    /// <summary>Convert track quad tree file (cqtc) to glTF.</summary>
    /// <param name="filePath">The track quad tree file path.</param>
    /// <param name="outputFilePath">-o, The output file path.</param>
    public void CqToGltf([Argument] string filePath, string? outputFilePath = null)
    {
        logger.LogInformation("Converting cqtc to glTF: {FileName}", Path.GetFileName(filePath));
        var data = File.ReadAllBytes(filePath);

        var typeInfo = CQuadTreeFile.Identify(data);
        logger.LogInformation("Quad tree type is {TypeInfo}", typeInfo);

        var quadTree = new CQuadTreeFile(data);
        var gltf = TrackGroundGltfConverter.Convert(quadTree);
        outputFilePath ??= filePath + GlbExtension;
        gltf.Save(outputFilePath);
        logger.LogInformation("Success, cqtc converted: {FileName}", Path.GetFileName(outputFilePath));
    }

    /// <summary>Convert glTF file to track quad tree (cqtc).</summary>
    /// <param name="filePath">The glTF file path.</param>
    /// <param name="type">-t, The type of quad tree.</param>
    /// <param name="outputFilePath">-o, The output file path.</param>
    public void GltfToCq([Argument] string filePath, CQuadTreeType type, string? outputFilePath = null)
    {
        logger.LogInformation("Converting glTF to cqtc: {FileName}", Path.GetFileName(filePath));
        var gltf = ModelRoot.Load(filePath);
        var quadTree = GltfTrackGroundConverter.Convert(gltf, CQuadTreeTypeInfo.Get(type));

        outputFilePath ??= filePath + ".cqtc";
        File.WriteAllBytes(outputFilePath, quadTree.Bytes);
        logger.LogInformation("Success, glTF converted: {FileName}", Path.GetFileName(outputFilePath));
    }
}
