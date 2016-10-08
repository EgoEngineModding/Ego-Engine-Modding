using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Graphics
{
    public struct DDSHeaderDXT10
    {
        public DXGI_Format dxgiFormat;
        public D3D10_Resource_Dimension resourceDimension;
        public UInt32 miscFlag;
        public UInt32 arraySize;
        public UInt32 miscFlags2;
    }
}
