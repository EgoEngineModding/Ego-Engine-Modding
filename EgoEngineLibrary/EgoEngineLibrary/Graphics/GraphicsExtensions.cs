using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Archive.Erp.Data;
using EgoEngineLibrary.Graphics.Dds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EgoEngineLibrary.Graphics
{
    public static class GraphicsExtensions
    {
        public static DdsFile ToDdsFile(this ErpGfxSRVResource srvRes, string mipMapsFilePath, bool exportTexArray, uint texArrayIndex)
        {
            DdsFile dds = new DdsFile();

            dds.header10.arraySize = srvRes.SurfaceRes.Fragment0.ArraySize;
            switch (srvRes.SurfaceRes.Fragment0.ImageType)
            {
                //case (ErpGfxSurfaceFormat)14: // gameparticles k_smoke; application
                case ErpGfxSurfaceFormat.ABGR8:
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;

                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                        dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_R8G8B8A8_UNORM;
                    }
                    else
                    {
                        dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_ALPHAPIXELS | DdsPixelFormat.Flags.DDPF_RGB;
                        dds.header.ddspf.fourCC = 0;
                        dds.header.ddspf.rGBBitCount = 32;
                        dds.header.ddspf.rBitMask = 0xFF;
                        dds.header.ddspf.gBitMask = 0xFF00;
                        dds.header.ddspf.bBitMask = 0xFF0000;
                        dds.header.ddspf.aBitMask = 0xFF000000;
                    }
                    break;
                case ErpGfxSurfaceFormat.DXT1: // ferrari_wheel_sfc
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;

                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                        dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC1_UNORM;
                    }
                    else
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT1"), 0);
                    }
                    break;
                case ErpGfxSurfaceFormat.DXT1_SRGB: // ferrari_wheel_df, ferrari_paint
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB;
                    break;
                case ErpGfxSurfaceFormat.DXT5: // ferrari_sfc
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;

                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                        dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC3_UNORM;
                    }
                    else
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT5"), 0);
                    }
                    break;
                case ErpGfxSurfaceFormat.DXT5_SRGB: // ferrari_decal
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB;
                    break;
                case ErpGfxSurfaceFormat.ATI1: // gameparticles k_smoke
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;

                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                        dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC4_UNORM;
                    }
                    else
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("ATI1"), 0);
                    }
                    break;
                case ErpGfxSurfaceFormat.ATI2: // ferrari_wheel_nm
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;

                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                        dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC5_UNORM;
                    }
                    else
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("ATI2"), 0);
                    }
                    break;
                case ErpGfxSurfaceFormat.BC6: // key0_2016; environment abu_dhabi tree_palm_06
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC6H_UF16;
                    break;
                case ErpGfxSurfaceFormat.BC7:
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC7_UNORM;
                    break;
                case ErpGfxSurfaceFormat.BC7_SRGB: // flow_boot splash_bg_image
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB;
                    break;
                default:
                    throw new NotSupportedException("Image format not supported!");
            }

            byte[] imageData = srvRes.SurfaceRes.Fragment1.Data;
            bool foundMipMapFile = !string.IsNullOrWhiteSpace(mipMapsFilePath) && File.Exists(mipMapsFilePath);
            if (srvRes.SurfaceRes.HasMips && srvRes.SurfaceRes.HasValidMips && foundMipMapFile)
            {
                using (MemoryStream output = new MemoryStream())
                using (ErpBinaryReader reader = new ErpBinaryReader(File.Open(mipMapsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    foreach (var mip in srvRes.SurfaceRes.Frag2.Mips)
                    {
                        byte[] mipData = reader.ReadBytes((int)mip.PackedSize);
                        if (mip.PackedSize != (ulong)mipData.LongLength)
                        {
                            throw new FileFormatException($"There is a mismatch with the mipmaps file.{Environment.NewLine}It is either incorrectly modded, or in the wrong folder.");
                        }

                        switch (mip.Compression)
                        {
                            case ErpCompressionAlgorithm.None:
                                output.Write(mipData, 0, mipData.Length);
                                break;
                            case ErpCompressionAlgorithm.LZ4:
                                mipData = LZ4.LZ4Codec.Decode(mipData, 0, mipData.Length, (int)mip.Size);
                                output.Write(mipData, 0, mipData.Length);
                                break;
                            default:
                                throw new NotSupportedException($"MipMap compression type {mip.Compression} is not supported!");
                        }
                    }

                    output.Write(imageData, 0, imageData.Length);
                    dds.bdata = output.ToArray();
                }

                dds.header.width = srvRes.SurfaceRes.MipWidth;
                dds.header.height = srvRes.SurfaceRes.MipHeight;
                dds.header.pitchOrLinearSize = (uint)srvRes.SurfaceRes.MipLinearSize;

                if (srvRes.SurfaceRes.Frag2.Mips.Count > 0)
                {
                    dds.header.flags |= DdsHeader.Flags.DDSD_MIPMAPCOUNT;
                    dds.header.mipMapCount = srvRes.SurfaceRes.Fragment0.MipMapCount + (uint)srvRes.SurfaceRes.Frag2.Mips.Count;
                    dds.header.caps |= DdsHeader.Caps.DDSCAPS_MIPMAP | DdsHeader.Caps.DDSCAPS_COMPLEX;
                }
            }
            else
            {
                dds.header.width = srvRes.SurfaceRes.Width;
                dds.header.height = srvRes.SurfaceRes.Height;
                dds.header.pitchOrLinearSize = srvRes.SurfaceRes.LinearSize;

                if (srvRes.SurfaceRes.Fragment0.MipMapCount > 0)
                {
                    dds.header.flags |= DdsHeader.Flags.DDSD_MIPMAPCOUNT;
                    dds.header.mipMapCount = srvRes.SurfaceRes.Fragment0.MipMapCount;
                    dds.header.caps |= DdsHeader.Caps.DDSCAPS_MIPMAP | DdsHeader.Caps.DDSCAPS_COMPLEX;
                }

                if (srvRes.SurfaceRes.Fragment0.ArraySize > 1)
                {
                    uint bytesPerArrayImage = (uint)imageData.Length / srvRes.SurfaceRes.Fragment0.ArraySize;
                    byte[] data = new byte[bytesPerArrayImage];

                    if (!exportTexArray)
                    {
                        dds.header10.arraySize = 1;
                        Buffer.BlockCopy(imageData, (int)(bytesPerArrayImage * texArrayIndex), data, 0, (int)bytesPerArrayImage);
                        dds.bdata = data;
                    }
                    else
                    {
                        dds.bdata = imageData;

                        // TODO: Add support for exporting individual tex array slices
                        //string output = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
                        //for (int i = 0; i < srvRes.SurfaceRes.Fragment0.ArraySize; ++i)
                        //{
                        //    Buffer.BlockCopy(imageData, (int)(bytesPerArrayImage * i), data, 0, (int)bytesPerArrayImage);
                        //    dds.bdata = data;
                        //    dds.Write(File.Open(output + "!!!" + i.ToString("000") + ".dds", FileMode.Create, FileAccess.Write, FileShare.Read), -1);
                        //}
                    }
                }
                else
                {
                    dds.bdata = imageData;
                }
            }

            return dds;
        }
    }
}
