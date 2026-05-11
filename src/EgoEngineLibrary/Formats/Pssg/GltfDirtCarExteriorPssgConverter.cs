using EgoEngineLibrary.Graphics.Pssg;
using EgoEngineLibrary.Graphics.Pssg.Elements;

namespace EgoEngineLibrary.Formats.Pssg
{
    public sealed class GltfDirtCarExteriorPssgConverter : GltfCarInteriorPssgConverter
    {
        private class DirtImportState : ImportState
        {
            public DirtImportState(PssgElement rdsLib, PssgElement ribLib, Dictionary<string, ShaderInputInfo> shaderGroupMap)
                : base(rdsLib, ribLib, shaderGroupMap)
            {
            }

            public override PssgNode CreateRenderNode(PssgFile pssg, PssgElement? parent)
            {
                return new PssgVisibleRenderNode(pssg, parent);
            }
        }

        public static new bool SupportsPssg(PssgFile pssg)
        {
            return pssg.Elements<PssgVisibleRenderNode>().Any(x => x.IsExactType<PssgVisibleRenderNode>());
        }

        protected override ImportState CreateState(PssgElement rdsLib, PssgElement ribLib, Dictionary<string, ShaderInputInfo> shaderGroupMap)
        {
            return new DirtImportState(rdsLib, ribLib, shaderGroupMap);
        }
    }
}
