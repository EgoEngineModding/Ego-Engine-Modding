using EgoEngineLibrary.Graphics.Pssg;
using EgoEngineLibrary.Graphics.Pssg.Elements;

namespace EgoEngineLibrary.Formats.Pssg
{
	// Dirt 1 car exterior is similar to interior except it uses VISIBLERENDERNODE instead
	public sealed class DirtCarExteriorPssgGltfConverter : CarInteriorPssgGltfConverter
	{
        private class DirtExportState : ExportState
        {
            public override bool IsRenderNode(PssgNode element)
            {
                return element.IsExactType<PssgVisibleRenderNode>();
            }
        }

        public static new bool SupportsPssg(PssgFile pssg)
        {
            return pssg.Elements<PssgVisibleRenderNode>().Any(x => x.IsExactType<PssgVisibleRenderNode>());
        }

        protected override ExportState CreateState()
        {
            return new DirtExportState();
        }
	}
}
