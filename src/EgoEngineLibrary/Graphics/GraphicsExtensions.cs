using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Archive.Erp.Data;
using EgoEngineLibrary.Graphics.Dds;
using K4os.Compression.LZ4;
using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EgoEngineLibrary.Graphics
{
    public static class GraphicsExtensions
    {
        public static DdsFile ToDdsFile(this ErpGfxSRVResource srvRes, Stream? mipMapsStream, bool exportTexArray, uint texArrayIndex)
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
                case ErpGfxSurfaceFormat.BC2_SRGB:
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB;
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
                    throw new NotSupportedException("Image format not supported");
            }

            byte[] imageData = srvRes.SurfaceRes.Fragment1.Data;
            if (srvRes.SurfaceRes.HasMips && srvRes.SurfaceRes.HasValidMips && mipMapsStream is not null)
            {
                using (MemoryStream output = new MemoryStream())
                using (BinaryReader reader = new BinaryReader(mipMapsStream, Encoding.ASCII, true))
                {
                    foreach (var mip in srvRes.SurfaceRes.Frag2.Mips)
                    {
                        byte[] mipData = reader.ReadBytes((int)mip.PackedSize);
                        if (mip.PackedSize != (ulong)mipData.LongLength)
                        {
                            throw new FileFormatException($"The mipmaps file, and erp data do not match. (compressed size)");
                        }

                        switch (mip.Compression)
                        {
                            case ErpCompressionAlgorithm.None:
                                output.Write(mipData, 0, mipData.Length);
                                break;
                            case ErpCompressionAlgorithm.LZ4:
                                byte[] decompressedMipData = new byte[(int)mip.Size];
                                int decompSize = LZ4Codec.Decode(mipData, 0, mipData.Length, decompressedMipData, 0, decompressedMipData.Length);
                                if (decompSize < 0)
                                {
                                    throw new InvalidDataException($"The mipmaps file, and erp data do not match. (uncompressed size)");
                                }
                                output.Write(decompressedMipData, 0, decompSize);
                                break;
                            default:
                                throw new NotSupportedException($"MipMap compression type {mip.Compression} is not supported");
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

        public static void ToErpGfxSRVResource(this DdsFile dds, ErpGfxSRVResource srvRes, Stream mipMapsStream, bool importTexArray, uint texArrayIndex)
        {
            if (srvRes.SurfaceRes.HasMips && mipMapsStream == null)
            {
                throw new ArgumentNullException(nameof(mipMapsStream));
            }

            ErpGfxSurfaceFormat imageType = dds.GetErpGfxSurfaceFormat();
            uint mipLinearSize = dds.GetLinearSize();

            if (srvRes.SurfaceRes.HasMips)
            {
                int mipCount = (int)dds.header.mipMapCount / 4;
                if (srvRes.SurfaceRes.Frag2.Mips.Count < dds.header.mipMapCount) mipCount = srvRes.SurfaceRes.Frag2.Mips.Count;
                bool identicalMipCount = mipCount == srvRes.SurfaceRes.Frag2.Mips.Count;

                dds.header.mipMapCount -= (uint)mipCount;
                uint div = (uint)Math.Pow(2.0, mipCount);
                dds.header.width /= div;
                dds.header.height /= div;

                UInt64 offset = 0;
                bool compress2017 = srvRes.SurfaceRes.Frag2.Mips.Exists(x => x.Compression != ErpCompressionAlgorithm.None);
                List<ErpGfxSurfaceRes2Mips> newMips = new List<ErpGfxSurfaceRes2Mips>(mipCount);
                using (MemoryStream ddsDataStream = new MemoryStream(dds.bdata, false))
                using (BinaryReader reader = new BinaryReader(ddsDataStream))
                using (BinaryWriter writer = new BinaryWriter(mipMapsStream, Encoding.ASCII, true))
                {
                    for (int i = 0; i < mipCount; ++i)
                    {
                        ErpGfxSurfaceRes2Mips mip = new ErpGfxSurfaceRes2Mips();

                        if (identicalMipCount)
                        {
                            mip.Compression = srvRes.SurfaceRes.Frag2.Mips[i].Compression;
                        }
                        else if (compress2017)
                        {
                            mip.Compression = i % 2 == 0 ? ErpCompressionAlgorithm.LZ4 : ErpCompressionAlgorithm.None;
                        }
                        else
                        {
                            mip.Compression = ErpCompressionAlgorithm.None;
                        }
                        mip.Offset = offset;

                        byte[] mipData = reader.ReadBytes((int)mipLinearSize);
                        ulong mipPackedSize = (ulong)mipData.Length;
                        switch (mip.Compression)
                        {
                            case ErpCompressionAlgorithm.LZ4:
                                byte[] compressedMipData = new byte[LZ4Codec.MaximumOutputSize(mipData.Length)];
                                int compSize = LZ4Codec.Encode(mipData, 0, mipData.Length, compressedMipData, 0, compressedMipData.Length, LZ4Level.L12_MAX);
                                writer.Write(compressedMipData, 0, compSize);
                                mipPackedSize = (ulong)compSize;
                                break;
                            case ErpCompressionAlgorithm.None:
                                writer.Write(mipData);
                                break;
                            default:
                                throw new NotSupportedException($"MipMap compression type {mip.Compression} is not supported");
                        }

                        mip.PackedSize = mipPackedSize;
                        mip.Size = mipLinearSize;

                        offset += mipPackedSize;
                        mipLinearSize /= 4;

                        newMips.Add(mip);
                    }

                    srvRes.SurfaceRes.Fragment1.Data = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                }

                srvRes.SurfaceRes.Frag2.Mips = newMips;
            }
            else
            {
                byte[] imageByteData;
                if (srvRes.SurfaceRes.Fragment0.ArraySize > 1)
                {
                    uint bytesPerArrayImage = (uint)srvRes.SurfaceRes.Fragment1.Data.Length / srvRes.SurfaceRes.Fragment0.ArraySize;

                    if (!importTexArray)
                    {
                        Buffer.BlockCopy(dds.bdata, 0, srvRes.SurfaceRes.Fragment1.Data, (int)(bytesPerArrayImage * texArrayIndex), (int)bytesPerArrayImage);
                    }
                    else
                    {
                        if (dds.header10.arraySize <= 1)
                        {
                            throw new FileFormatException("The texture array size must be greater than 1");
                        }

                        imageByteData = dds.bdata;
                        srvRes.SurfaceRes.Fragment1.Data = imageByteData;
                        srvRes.SurfaceRes.Fragment0.ArraySize = dds.header10.arraySize;

                        // TODO: Add support for importing individual tex array slices
                        //imageByteData = new byte[bytesPerArrayImage];
                        //string input = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
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
                    imageByteData = dds.bdata;
                    srvRes.SurfaceRes.Fragment1.Data = imageByteData;
                }
            }

            srvRes.Fragment0.ImageType = imageType;
            srvRes.Fragment0.MipMapCount = dds.header.mipMapCount;

            srvRes.SurfaceRes.Fragment0.ImageType = imageType;
            srvRes.SurfaceRes.Fragment0.Width = dds.header.width;
            srvRes.SurfaceRes.Fragment0.Height = dds.header.height;
            srvRes.SurfaceRes.Fragment0.MipMapCount = dds.header.mipMapCount;
        }
        public static ErpGfxSurfaceFormat GetErpGfxSurfaceFormat(this DdsFile dds)
        {
            ErpGfxSurfaceFormat imageType;

            switch (dds.header.ddspf.fourCC)
            {
                case 0:
                    imageType = ErpGfxSurfaceFormat.ABGR8;
                    break;
                case 827611204: // DXT1 aka DXGI_FORMAT_BC1_UNORM
                    imageType = ErpGfxSurfaceFormat.DXT1;
                    break;
                case 894720068: // DXT5 aka DXGI_FORMAT_BC3_UNORM
                    imageType = ErpGfxSurfaceFormat.DXT5;
                    break;
                case 1429488450: // BC4U from Intel® Texture Works Plugin for Photoshop
                case 826889281: // ATI1
                    imageType = ErpGfxSurfaceFormat.ATI1;
                    break;
                case 1429553986: // BC5U from Intel® Texture Works Plugin for Photoshop
                case 843666497: // ATI2 aka DXGI_FORMAT_BC5_UNORM
                    imageType = ErpGfxSurfaceFormat.ATI2;
                    break;
                case 808540228: // DX10
                    if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_UNORM)
                    {
                        imageType = ErpGfxSurfaceFormat.BC7;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB)
                    {
                        imageType = ErpGfxSurfaceFormat.BC7_SRGB;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_UNORM ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_SNORM ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_UINT ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_SINT)
                    {
                        goto case 0;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM)
                    {
                        goto case 827611204;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB)
                    {
                        imageType = ErpGfxSurfaceFormat.DXT1_SRGB;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM)
                    {
                        goto case 894720068;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB)
                    {
                        imageType = ErpGfxSurfaceFormat.DXT5_SRGB;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB)
                    {
                        imageType = ErpGfxSurfaceFormat.BC2_SRGB;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC4_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC4_UNORM ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC4_SNORM)
                    {
                        goto case 826889281;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC5_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC5_UNORM ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC5_SNORM)
                    {
                        goto case 843666497;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC6H_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC6H_UF16 ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC6H_SF16)
                    {
                        imageType = ErpGfxSurfaceFormat.BC6;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                default:
                    throw new NotSupportedException("Image format not supported");
            }

            return imageType;
        }
        
        public static DdsFile ToDdsFile(this PssgNode node, bool cubePreview)
        {
            DdsFile dds = new DdsFile();

            dds.header.height = (uint)(node.Attributes["height"].Value);
            dds.header.width = (uint)(node.Attributes["width"].Value);
            switch ((string)node.Attributes["texelFormat"].Value)
            {
                // gimp doesn't like pitch, so we'll go with linear size
                case "dxt1":
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value) / 2;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes(((string)node.Attributes["texelFormat"].Value).ToUpper()), 0);
                    break;
                case "dxt1_srgb":
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value) / 2;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB;
                    break;
                case "dxt2":
                case "dxt3":
                case "dxt4":
                case "dxt5":
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes(((string)node.Attributes["texelFormat"].Value).ToUpper()), 0);
                    break;
                case "dxt3_srgb":
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB;
                    break;
                case "dxt5_srgb":
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB;
                    break;
                case "BC7":
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC7_UNORM;
                    break;
                case "BC7_srgb":
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB;
                    break;
                case "ui8x4":
                case "u8x4":
                    dds.header.flags |= DdsHeader.Flags.DDSD_PITCH;
                    dds.header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value); // is this right?
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_ALPHAPIXELS | DdsPixelFormat.Flags.DDPF_RGB;
                    dds.header.ddspf.fourCC = 0;
                    dds.header.ddspf.rGBBitCount = 32;
                    dds.header.ddspf.rBitMask = 0xFF0000;
                    dds.header.ddspf.gBitMask = 0xFF00;
                    dds.header.ddspf.bBitMask = 0xFF;
                    dds.header.ddspf.aBitMask = 0xFF000000;
                    break;
                case "u8":
                    dds.header.flags |= DdsHeader.Flags.DDSD_PITCH;
                    dds.header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value); // is this right?
                    // Interchanging the commented values will both work, not sure which is better
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_LUMINANCE;
                    //header.ddspf.flags |= DDS_PIXELFORMAT.Flags.DDPF_ALPHA;
                    dds.header.ddspf.fourCC = 0;
                    dds.header.ddspf.rGBBitCount = 8;
                    dds.header.ddspf.rBitMask = 0xFF;
                    //header.ddspf.aBitMask = 0xFF;
                    break;
                default:
                    throw new NotSupportedException("Texel format not supported.");
            }

            // Mip Maps
            if (node.HasAttribute("automipmap") == true && node.HasAttribute("numberMipMapLevels") == true)
            {
                if ((uint)node.Attributes["automipmap"].Value == 0 && (uint)node.Attributes["numberMipMapLevels"].Value > 0)
                {
                    dds.header.flags |= DdsHeader.Flags.DDSD_MIPMAPCOUNT;
                    dds.header.mipMapCount = (uint)((uint)node.Attributes["numberMipMapLevels"].Value + 1);
                    dds.header.caps |= DdsHeader.Caps.DDSCAPS_MIPMAP | DdsHeader.Caps.DDSCAPS_COMPLEX;
                }
            }

            // Byte Data
            List<PssgNode> textureImageBlocks = node.FindNodes("TEXTUREIMAGEBLOCK");
            if ((uint)node.Attributes["imageBlockCount"].Value > 1)
            {
                dds.bdata2 = new Dictionary<int, byte[]>();
                for (int i = 0; i < textureImageBlocks.Count; i++)
                {
                    switch (textureImageBlocks[i].Attributes["typename"].ToString())
                    {
                        case "Raw":
                            dds.header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEX;
                            dds.bdata2.Add(0, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                        case "RawNegativeX":
                            dds.header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEX;
                            dds.bdata2.Add(1, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                        case "RawPositiveY":
                            dds.header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEY;
                            dds.bdata2.Add(2, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                        case "RawNegativeY":
                            dds.header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEY;
                            dds.bdata2.Add(3, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                        case "RawPositiveZ":
                            dds.header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEZ;
                            dds.bdata2.Add(4, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                        case "RawNegativeZ":
                            dds.header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEZ;
                            dds.bdata2.Add(5, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                    }
                }
                if (cubePreview == true)
                {
                    dds.header.caps2 = 0;
                }
                else if (dds.bdata2.Count == (uint)node.Attributes["imageBlockCount"].Value)
                {
                    dds.header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP;
                    dds.header.flags = dds.header.flags ^ DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = 0;
                    dds.header.caps |= DdsHeader.Caps.DDSCAPS_COMPLEX;
                }
                else
                {
                    throw new Exception("Loading cubemap failed because not all blocks were found. (Read)");
                }
            }
            else
            {
                dds.bdata = (byte[])textureImageBlocks[0].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value;
            }

            return dds;
        }

        public static void ToPssgNode(this DdsFile dds, PssgNode node)
        {

            node.Attributes["height"].Value = dds.header.height;
            node.Attributes["width"].Value = dds.header.width;
            if (node.HasAttribute("numberMipMapLevels") == true)
            {
                if ((int)dds.header.mipMapCount - 1 >= 0)
                {
                    node.Attributes["numberMipMapLevels"].Value = dds.header.mipMapCount - 1;
                }
                else
                {
                    node.Attributes["numberMipMapLevels"].Value = 0u;
                }
            }
            if (dds.header.ddspf.rGBBitCount == 32)
            {
                node.Attributes["texelFormat"].Value = "ui8x4";
            }
            else if (dds.header.ddspf.rGBBitCount == 8)
            {
                node.Attributes["texelFormat"].Value = "u8";
            }
            else if (dds.header.ddspf.fourCC == 808540228) //DX10
            {
                if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_TYPELESS ||
                    dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_UNORM)
                {
                    node.Attributes["texelFormat"].Value = "BC7";
                }
                else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB)
                {
                    node.Attributes["texelFormat"].Value = "BC7_srgb";
                }
                else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_TYPELESS ||
                    dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM)
                {
                    node.Attributes["texelFormat"].Value = "dxt1";
                }
                else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB)
                {
                    node.Attributes["texelFormat"].Value = "dxt1_srgb";
                }
                else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC2_TYPELESS ||
                    dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC2_UNORM)
                {
                    node.Attributes["texelFormat"].Value = "dxt3";
                }
                else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB)
                {
                    node.Attributes["texelFormat"].Value = "dxt3_srgb";
                }
                else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_TYPELESS ||
                    dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM)
                {
                    node.Attributes["texelFormat"].Value = "dxt5";
                }
                else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB)
                {
                    node.Attributes["texelFormat"].Value = "dxt5_srgb";
                }
                else
                {
                    throw new FormatException("The dds has an invalid or unsupported format type!");
                }
            }
            else
            {
                node.Attributes["texelFormat"].Value = Encoding.UTF8.GetString(BitConverter.GetBytes(dds.header.ddspf.fourCC)).ToLower();
            }
            List<PssgNode> textureImageBlocks = node.FindNodes("TEXTUREIMAGEBLOCK");
            if (dds.bdata2 != null && dds.bdata2.Count > 0)
            {
                for (int i = 0; i < textureImageBlocks.Count; i++)
                {
                    switch (textureImageBlocks[i].Attributes["typename"].ToString())
                    {
                        case "Raw":
                            if (dds.bdata2.ContainsKey(0) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = dds.bdata2[0];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)dds.bdata2[0].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                        case "RawNegativeX":
                            if (dds.bdata2.ContainsKey(1) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = dds.bdata2[1];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)dds.bdata2[1].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                        case "RawPositiveY":
                            if (dds.bdata2.ContainsKey(2) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = dds.bdata2[2];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)dds.bdata2[2].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                        case "RawNegativeY":
                            if (dds.bdata2.ContainsKey(3) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = dds.bdata2[3];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)dds.bdata2[3].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                        case "RawPositiveZ":
                            if (dds.bdata2.ContainsKey(4) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = dds.bdata2[4];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)dds.bdata2[4].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                        case "RawNegativeZ":
                            if (dds.bdata2.ContainsKey(5) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = dds.bdata2[5];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)dds.bdata2[5].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                    }
                }
            }
            else
            {
                if ((uint)node.Attributes["imageBlockCount"].Value > 1)
                {
                    throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                }
                textureImageBlocks[0].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = dds.bdata;
                textureImageBlocks[0].Attributes["size"].Value = (UInt32)dds.bdata.Length;
            }
        }
    }
}
