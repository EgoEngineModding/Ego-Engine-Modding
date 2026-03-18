using EgoEngineLibrary.Graphics.Pssg;
using EgoEngineLibrary.Graphics.Pssg.Elements;

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
            var rdsNodes = pssg.Elements<PssgRenderDataSource>();
            foreach (var rdsNode in rdsNodes)
            {
                var info = GetShaderInfo(rdsNode, visitedShaders);
                if (info is not null)
                    inputInfos.Add(info);
            }

            return inputInfos;

            static ShaderInputInfo? GetShaderInfo(PssgRenderDataSource rdsElement, HashSet<string> visitedShaders)
            {
                var rdsId = rdsElement.Id;
                var risNode = rdsElement.File.Elements<PssgRenderInstanceSource>().FirstOrDefault(x => x.Source == '#' + rdsId);
                var siNode = (risNode?.ParentElement as PssgRenderInstance)?.TryGetShaderInstance();
                var sgNode = siNode?.TryGetShaderGroup();
                if (sgNode is null)
                    return null;
                if (visitedShaders.Contains(sgNode.Id))
                    return null;

                var dataBlockIdMap = new Dictionary<string, int>();
                var blockInputs = new List<ShaderBlockInputInfo>();
                var renderStreamNodes = rdsElement.Streams;
                foreach (var rsNode in renderStreamNodes)
                {
                    var subStream = rsNode.SubStream;

                    var dbNode = rsNode.GetDataBlock();
                    var dbStreamNode = dbNode.Streams.ElementAt(Convert.ToInt32(subStream));

                    var renderType = dbStreamNode.RenderType;
                    var offset = dbStreamNode.Offset;
                    var stride = dbStreamNode.Stride;
                    var dataType = dbStreamNode.DataType;

                    var vi = new ShaderVertexInputInfo(renderType, dataType, offset, stride);
                    if (dataBlockIdMap.TryGetValue(dbNode.Id, out var biIndex))
                    {
                        var bi = blockInputs[biIndex];
                        bi.VertexInputs.Add(vi);
                    }
                    else
                    {
                        var bi = new ShaderBlockInputInfo(new List<ShaderVertexInputInfo>());
                        bi.VertexInputs.Add(vi);
                        dataBlockIdMap.Add(dbNode.Id, blockInputs.Count);
                        blockInputs.Add(bi);
                    }
                }

                visitedShaders.Add(sgNode.Id);
                return new ShaderInputInfo(sgNode.Id, blockInputs);
            }
        }
    }
}
