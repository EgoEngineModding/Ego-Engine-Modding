using System;

namespace EgoEngineLibrary.Graphics.Dds;

public struct DdsHeader
{
    public UInt32 size;
    public Flags flags;
    public UInt32 height;
    public UInt32 width;
    public UInt32 pitchOrLinearSize;
    public UInt32 depth;
    public UInt32 mipMapCount;
    public UInt32[] reserved1; //  = new uint[11]
    public DdsPixelFormat ddspf;
    public Caps caps;
    public Caps2 caps2;
    public UInt32 caps3;
    public UInt32 caps4;
    public UInt32 reserved2;

    [Flags]
    public enum Flags : UInt32
    {
        DDSD_CAPS = 0x1,
        DDSD_HEIGHT = 0x2,
        DDSD_WIDTH = 0x4,
        DDSD_PITCH = 0x8,
        DDSD_PIXELFORMAT = 0x1000,
        DDSD_MIPMAPCOUNT = 0x20000,
        DDSD_LINEARSIZE = 0x80000,
        DDSD_DEPTH = 0x800000
    }

    [Flags]
    public enum Caps : UInt32
    {
        DDSCAPS_COMPLEX = 0x8,
        DDSCAPS_MIPMAP = 0x400000,
        DDSCAPS_TEXTURE = 0x1000
    }

    [Flags]
    public enum Caps2 : UInt32
    {
        DDSCAPS2_CUBEMAP = 0x200,
        DDSCAPS2_CUBEMAP_POSITIVEX = 0x400,
        DDSCAPS2_CUBEMAP_NEGATIVEX = 0x800,
        DDSCAPS2_CUBEMAP_POSITIVEY = 0x1000,
        DDSCAPS2_CUBEMAP_NEGATIVEY = 0x2000,
        DDSCAPS2_CUBEMAP_POSITIVEZ = 0x4000,
        DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x8000,
        DDSCAPS2_VOLUME = 0x200000
    }
}