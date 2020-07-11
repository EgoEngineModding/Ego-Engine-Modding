using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Archive.Erp.Data
{
    public enum ErpGfxSurfaceFormat
    {
        ABGR8 = 15,
        DXT1 = 52,
        DXT1_SRGB = 54,
        DXT5 = 55,
        DXT5_SRGB = 57,
        // not sure anymore which is BC2/DXT3 and which is BC3/DXT5
        BC2_SRGB = 62,
        ATI1 = 63,
        ATI2 = 65,
        BC6 = 67,
        BC7 = 69,
        BC7_SRGB = 70
    }
}
