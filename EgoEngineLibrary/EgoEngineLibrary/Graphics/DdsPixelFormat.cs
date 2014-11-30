namespace EgoEngineLibrary.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public struct DdsPixelFormat
    {
        public uint size;
        public Flags flags;
        public uint fourCC;
        public uint rGBBitCount;
        public uint rBitMask;
        public uint gBitMask;
        public uint bBitMask;
        public uint aBitMask;

        public enum Flags
        {
            DDPF_ALPHAPIXELS = 0x1,
            DDPF_ALPHA = 0x2,
            DDPF_FOURCC = 0x4,
            DDPF_RGB = 0x40,
            DDPF_YUV = 0x200,
            DDPF_LUMINANCE = 0x20000
        }
    }
}
