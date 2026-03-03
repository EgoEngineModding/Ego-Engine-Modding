using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EgoEngineLibrary.Graphics.Pssg;

namespace EgoEngineLibrary.Formats.Pssg
{
    public sealed class GltfDirtCarExteriorPssgConverter : GltfCarInteriorPssgConverter
    {
        private class DirtImportState : ImportState
        {
            public DirtImportState(PssgElement rdsLib, PssgElement ribLib, Dictionary<string, ShaderInputInfo> shaderGroupMap)
                : base(rdsLib, ribLib, shaderGroupMap)
            {
                RenderNodeName = "VISIBLERENDERNODE";
            }
        }

        public new static bool SupportsPssg(PssgFile pssg)
        {
            return pssg.FindElements("VISIBLERENDERNODE").Any();
        }

        protected override ImportState CreateState(PssgElement rdsLib, PssgElement ribLib, Dictionary<string, ShaderInputInfo> shaderGroupMap)
        {
            return new DirtImportState(rdsLib, ribLib, shaderGroupMap);
        }
    }
}
