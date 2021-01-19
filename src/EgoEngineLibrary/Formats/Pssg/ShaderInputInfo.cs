using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EgoEngineLibrary.Formats.Pssg
{
    public record ShaderBlockInputInfo(List<ShaderVertexInputInfo> VertexInputs);
    public record ShaderVertexInputInfo(string Name, string DataType, uint Offset, uint Stride);

    public class ShaderInputInfo
    {
        public string ShaderGroupId { get; }

        public IEnumerable<ShaderBlockInputInfo> BlockInputs { get; }

        public ShaderInputInfo(string shaderGroupId, IEnumerable<ShaderBlockInputInfo> blockInputInfo)
        {
            ShaderGroupId = shaderGroupId;
            BlockInputs = blockInputInfo;
        }

        public static List<ShaderInputInfo> CreateFromPssg(PssgFile pssg)
        {
            var visitedShaders = new HashSet<string>();
            var inputInfos = new List<ShaderInputInfo>();

            // Figure out the layout of the vertex data for each shader group by going through rds nodes
            var rdsNodes = pssg.FindNodes("RENDERDATASOURCE");
            foreach (var rdsNode in rdsNodes)
            {
                var info = GetShaderInfo(rdsNode, visitedShaders);
                if (info is not null)
                    inputInfos.Add(info);
            }

            return inputInfos;

            static ShaderInputInfo? GetShaderInfo(PssgNode rdsNode, HashSet<string> visitedShaders)
            {
                var rdsId = rdsNode.Attributes["id"].GetValue<string>();
                var risNode = rdsNode.File.FindNodes("RENDERINSTANCESOURCE", "source", '#' + rdsId).FirstOrDefault();
                var shaderInstanceId = risNode?.ParentNode?.Attributes["shader"].GetValue<string>().Substring(1);
                if (shaderInstanceId is null)
                    return null;

                var siNode = rdsNode.File.FindNodes("SHADERINSTANCE", "id", shaderInstanceId).FirstOrDefault();
                if (siNode is null)
                    return null;

                var shaderGroupId = siNode.Attributes["shaderGroup"].GetValue<string>().Substring(1);
                var sgNode = rdsNode.File.FindNodes("SHADERGROUP", "id", shaderGroupId).FirstOrDefault();
                if (sgNode is null)
                    return null;
                if (visitedShaders.Contains(shaderGroupId))
                    return null;

                var dataBlockIdMap = new Dictionary<string, int>();
                var blockInputs = new List<ShaderBlockInputInfo>();
                var renderStreamNodes = rdsNode.FindNodes("RENDERSTREAM");
                foreach (var rsNode in renderStreamNodes)
                {
                    var dbId = rsNode.Attributes["dataBlock"].GetValue<string>().Substring(1);
                    var subStream = rsNode.Attributes["subStream"].GetValue<uint>();

                    var dbNode = rsNode.File.FindNodes("DATABLOCK", "id", dbId).First();
                    var dbStreamNode = dbNode.ChildNodes[(int)subStream];

                    var renderType = dbStreamNode.Attributes["renderType"].GetValue<string>();
                    var offset = dbStreamNode.Attributes["offset"].GetValue<uint>();
                    var stride = dbStreamNode.Attributes["stride"].GetValue<uint>();
                    var dataType = dbStreamNode.Attributes["dataType"].GetValue<string>();

                    var vi = new ShaderVertexInputInfo(renderType, dataType, offset, stride);
                    if (dataBlockIdMap.TryGetValue(dbId, out var biIndex))
                    {
                        var bi = blockInputs[biIndex];
                        bi.VertexInputs.Add(vi);
                    }
                    else
                    {
                        var bi = new ShaderBlockInputInfo(new List<ShaderVertexInputInfo>());
                        bi.VertexInputs.Add(vi);
                        dataBlockIdMap.Add(dbId, blockInputs.Count);
                        blockInputs.Add(bi);
                    }
                }

                visitedShaders.Add(shaderGroupId);
                return new ShaderInputInfo(shaderGroupId, blockInputs);
            }
        }
    }
}
