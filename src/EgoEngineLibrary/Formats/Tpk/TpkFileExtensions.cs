using EgoEngineLibrary.Graphics.Dds;
using System;
using System.Text;

namespace EgoEngineLibrary.Formats.Tpk;

public static class TpkFileExtensions
{
    private const uint MinBc1LinearSize = 8;
    private const uint MinBc2LinearSize = 16;
    private const uint MinBc3LinearSize = 16;

    public static DdsFile ToDds(this TpkFile tpk)
    {
        var dds = new DdsFile();

        switch (tpk.Format)
        {
            case TpkImageFormat.Bgra: // BGRA little-endian
                dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_ALPHAPIXELS | DdsPixelFormat.Flags.DDPF_RGB;
                dds.header.ddspf.fourCC = 0;
                dds.header.ddspf.rGBBitCount = 32;
                dds.header.ddspf.bBitMask = 0xFF;
                dds.header.ddspf.gBitMask = 0xFF00;
                dds.header.ddspf.rBitMask = 0xFF0000;
                dds.header.ddspf.aBitMask = 0xFF000000;
                dds.header.pitchOrLinearSize = tpk.Width * tpk.Height * 4;
                break;
            case TpkImageFormat.Dxt1:
                dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT1"), 0);
                dds.header.pitchOrLinearSize = Math.Max(tpk.Width * tpk.Height / 2, MinBc1LinearSize);
                break;
            case TpkImageFormat.Dxt3:
                dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT3"), 0);
                dds.header.pitchOrLinearSize = Math.Max(tpk.Width * tpk.Height, MinBc2LinearSize);
                break;
            case TpkImageFormat.Dxt5:
                dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT5"), 0);
                dds.header.pitchOrLinearSize = Math.Max(tpk.Width * tpk.Height, MinBc3LinearSize);
                break;
            default:
                throw new NotSupportedException($"Tpk format {tpk.Format} not supported.");
        }

        dds.header.width = tpk.Width;
        dds.header.height = tpk.Height;

        if (tpk.MipMapCount > 0)
        {
            dds.header.flags |= DdsHeader.Flags.DDSD_MIPMAPCOUNT;
            dds.header.mipMapCount = tpk.MipMapCount;
            dds.header.caps |= DdsHeader.Caps.DDSCAPS_MIPMAP | DdsHeader.Caps.DDSCAPS_COMPLEX;
        }

        dds.bdata = tpk.Data;
        return dds;
    }

    public static void FromDds(this TpkFile tpk, DdsFile dds)
    {
        tpk.Format = GetTpkImageFormat(dds);
        tpk.Width = dds.header.width;
        tpk.Height = dds.header.height;
        tpk.MipMapCount = dds.header.mipMapCount;
        tpk.Data = dds.bdata;
    }
    public static TpkImageFormat GetTpkImageFormat(this DdsFile dds)
    {
        TpkImageFormat format;

        switch (dds.header.ddspf.fourCC)
        {
            case 0:
                format = TpkImageFormat.Bgra;
                break;
            case 827611204: // DXT1 aka DXGI_FORMAT_BC1_UNORM
                format = TpkImageFormat.Dxt1;
                break;
            case 861165636: // DXT5 aka DXGI_FORMAT_BC2_UNORM
                format = TpkImageFormat.Dxt3;
                break;
            case 894720068: // DXT5 aka DXGI_FORMAT_BC3_UNORM
                format = TpkImageFormat.Dxt5;
                break;
            case 808540228: // DX10
                if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_B8G8R8A8_TYPELESS ||
                    dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_B8G8R8A8_UNORM)
                {
                    goto case 0;
                }
                else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_TYPELESS ||
                    dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM)
                {
                    goto case 827611204;
                }
                else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC2_TYPELESS ||
                    dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC2_UNORM)
                {
                    goto case 861165636;
                }
                else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_TYPELESS ||
                    dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM)
                {
                    goto case 894720068;
                }
                else
                {
                    goto default;
                }
            default:
                throw new NotSupportedException("DDS image format not supported");
        }

        return format;
    }
}
