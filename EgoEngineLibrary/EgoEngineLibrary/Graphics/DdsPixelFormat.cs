namespace EgoEngineLibrary.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public struct DdsPixelFormat
    {
        public UInt32 size;
        public Flags flags;
        public UInt32 fourCC;
        public UInt32 rGBBitCount;
        public UInt32 rBitMask;
        public UInt32 gBitMask;
        public UInt32 bBitMask;
        public UInt32 aBitMask;

        public enum Flags : UInt32
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
