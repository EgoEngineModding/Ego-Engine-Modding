using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Formats.Pssg
{
	// Dirt 1 car exterior is similar to interior except it uses VISIBLERENDERNODE instead
	public sealed class DirtCarExteriorPssgGltfConverter : CarInteriorPssgGltfConverter
	{
        private class DirtExportState : ExportState
        {
            public DirtExportState()
            {
                RenderNodeName = "VISIBLERENDERNODE";
            }
        }

        public new static bool SupportsPssg(PssgFile pssg)
        {
            return pssg.FindNodes("VISIBLERENDERNODE").Any();
        }

        protected override ExportState CreateState()
        {
            return new DirtExportState();
        }
	}
}
