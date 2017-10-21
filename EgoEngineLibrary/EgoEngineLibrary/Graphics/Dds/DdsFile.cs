namespace EgoEngineLibrary.Graphics.Dds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class DdsFile
    {
        UInt32 magic;
        public DdsHeader header;
        public DDSHeaderDXT10 header10;
        public byte[] bdata { get; set; }
        Dictionary<int, byte[]> bdata2;

        public DdsFile()
        {
            magic = 0x20534444;
            header.size = 124;
            header.flags |= DdsHeader.Flags.DDSD_CAPS | DdsHeader.Flags.DDSD_HEIGHT | DdsHeader.Flags.DDSD_WIDTH | DdsHeader.Flags.DDSD_PIXELFORMAT;
            header.reserved1 = new uint[11];
            header.ddspf.size = 32;
            header.caps |= DdsHeader.Caps.DDSCAPS_TEXTURE;
            header10.resourceDimension = D3D10_Resource_Dimension.D3D10_RESOURCE_DIMENSION_TEXTURE2D;
            header10.arraySize = 1;
        }
        public DdsFile(PssgNode node, bool cubePreview)
            : this()
        {
            header.height = (uint)(node.Attributes["height"].Value);
            header.width = (uint)(node.Attributes["width"].Value);
            switch ((string)node.Attributes["texelFormat"].Value)
            {
                    // gimp doesn't like pitch, so we'll go with linear size
                case "dxt1":
                    header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value) / 2;
                    header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes(((string)node.Attributes["texelFormat"].Value).ToUpper()), 0);
                    break;
                case "dxt1_srgb":
                    header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value) / 2;
                    header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB;
                    break;
                case "dxt2":
                case "dxt3":
                case "dxt4":
                case "dxt5":
                    header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value);
                    header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes(((string)node.Attributes["texelFormat"].Value).ToUpper()), 0);
                    break;
                case "dxt3_srgb":
                    header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value);
                    header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB;
                    break;
                case "dxt5_srgb":
                    header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value);
                    header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB;
                    break;
                case "BC7":
                    header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value);
                    header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC7_UNORM;
                    break;
                case "BC7_srgb":
                    header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value);
                    header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB;
                    break;
                case "ui8x4":
                    header.flags |= DdsHeader.Flags.DDSD_PITCH;
                    header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value); // is this right?
                    header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_ALPHAPIXELS | DdsPixelFormat.Flags.DDPF_RGB;
                    header.ddspf.fourCC = 0;
                    header.ddspf.rGBBitCount = 32;
                    header.ddspf.rBitMask = 0xFF0000;
                    header.ddspf.gBitMask = 0xFF00;
                    header.ddspf.bBitMask = 0xFF;
                    header.ddspf.aBitMask = 0xFF000000;
                    break;
                case "u8":
                    header.flags |= DdsHeader.Flags.DDSD_PITCH;
                    header.pitchOrLinearSize = ((uint)node.Attributes["height"].Value * (uint)node.Attributes["width"].Value); // is this right?
                    // Interchanging the commented values will both work, not sure which is better
                    header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_LUMINANCE;
                    //header.ddspf.flags |= DDS_PIXELFORMAT.Flags.DDPF_ALPHA;
                    header.ddspf.fourCC = 0;
                    header.ddspf.rGBBitCount = 8;
                    header.ddspf.rBitMask = 0xFF;
                    //header.ddspf.aBitMask = 0xFF;
                    break;
            }

            // Mip Maps
            if (node.HasAttribute("automipmap") == true && node.HasAttribute("numberMipMapLevels") == true)
            {
                if ((uint)node.Attributes["automipmap"].Value == 0 && (uint)node.Attributes["numberMipMapLevels"].Value > 0)
                {
                    header.flags |= DdsHeader.Flags.DDSD_MIPMAPCOUNT;
                    header.mipMapCount = (uint)((uint)node.Attributes["numberMipMapLevels"].Value + 1);
                    header.caps |= DdsHeader.Caps.DDSCAPS_MIPMAP | DdsHeader.Caps.DDSCAPS_COMPLEX;
                }
            }

            // Byte Data
            List<PssgNode> textureImageBlocks = node.FindNodes("TEXTUREIMAGEBLOCK");
            if ((uint)node.Attributes["imageBlockCount"].Value > 1)
            {
                bdata2 = new Dictionary<int, byte[]>();
                for (int i = 0; i < textureImageBlocks.Count; i++)
                {
                    switch (textureImageBlocks[i].Attributes["typename"].ToString())
                    {
                        case "Raw":
                            header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEX;
                            bdata2.Add(0, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                        case "RawNegativeX":
                            header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEX;
                            bdata2.Add(1, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                        case "RawPositiveY":
                            header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEY;
                            bdata2.Add(2, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                        case "RawNegativeY":
                            header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEY;
                            bdata2.Add(3, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                        case "RawPositiveZ":
                            header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEZ;
                            bdata2.Add(4, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                        case "RawNegativeZ":
                            header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEZ;
                            bdata2.Add(5, (byte[])textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value);
                            break;
                    }
                }
                if (cubePreview == true)
                {
                    header.caps2 = 0;
                }
                else if (bdata2.Count == (uint)node.Attributes["imageBlockCount"].Value)
                {
                    header.caps2 |= DdsHeader.Caps2.DDSCAPS2_CUBEMAP;
                    header.flags = header.flags ^ DdsHeader.Flags.DDSD_LINEARSIZE;
                    header.pitchOrLinearSize = 0;
                    header.caps |= DdsHeader.Caps.DDSCAPS_COMPLEX;
                }
                else
                {
                    throw new Exception("Loading cubemap failed because not all blocks were found. (Read)");
                }
            }
            else
            {
                bdata = (byte[])textureImageBlocks[0].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value;
            }
        }
        public DdsFile(System.IO.Stream fileStream)
        {
            using (System.IO.BinaryReader b = new System.IO.BinaryReader(fileStream))
            {
                b.BaseStream.Position = 8;
                header.flags = (DdsHeader.Flags)b.ReadUInt32();
                header.height = b.ReadUInt32();
                header.width = b.ReadUInt32();
                header.pitchOrLinearSize = b.ReadUInt32();
                header.depth = b.ReadUInt32();
                header.mipMapCount = b.ReadUInt32();
                b.BaseStream.Position += 48;
                header.ddspf.flags = (DdsPixelFormat.Flags)b.ReadUInt32();
                header.ddspf.fourCC = b.ReadUInt32();
                header.ddspf.rGBBitCount = b.ReadUInt32();
                header.ddspf.rBitMask = b.ReadUInt32();
                header.ddspf.gBitMask = b.ReadUInt32();
                header.ddspf.bBitMask = b.ReadUInt32();
                header.ddspf.aBitMask = b.ReadUInt32();
                header.caps = (DdsHeader.Caps)b.ReadUInt32();
                header.caps2 = (DdsHeader.Caps2)b.ReadUInt32();
                b.BaseStream.Position += 12;
                if (header.ddspf.flags.HasFlag(DdsPixelFormat.Flags.DDPF_FOURCC) && header.ddspf.fourCC == 808540228) // DX10
                {
                    header10.dxgiFormat = (DXGI_Format)b.ReadUInt32();
                    header10.resourceDimension = (D3D10_Resource_Dimension)b.ReadUInt32();
                    header10.miscFlag = b.ReadUInt32();
                    header10.arraySize = b.ReadUInt32();
                    header10.miscFlags2 = b.ReadUInt32();
                }
                int count = 0;
                if ((uint)header.caps2 != 0)
                {
                    bdata2 = new Dictionary<int, byte[]>();
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEX) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEX)
                    {
                        count++;
                        bdata2.Add(0, null);
                    }
                    else
                    {
                        bdata2.Add(-1, null);
                    }
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEX) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEX)
                    {
                        count++;
                        bdata2.Add(1, null);
                    }
                    else
                    {
                        bdata2.Add(-2, null);
                    }
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEY) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEY)
                    {
                        count++;
                        bdata2.Add(2, null);
                    }
                    else
                    {
                        bdata2.Add(-3, null);
                    }
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEY) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEY)
                    {
                        count++;
                        bdata2.Add(3, null);
                    }
                    else
                    {
                        bdata2.Add(-4, null);
                    }
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEZ) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEZ)
                    {
                        count++;
                        bdata2.Add(4, null);
                    }
                    else
                    {
                        bdata2.Add(-5, null);
                    }
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEZ) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEZ)
                    {
                        count++;
                        bdata2.Add(5, null);
                    }
                    else
                    {
                        bdata2.Add(-6, null);
                    }
                    if (count > 0)
                    {
                        int length = (int)((b.BaseStream.Length - (long)128) / (long)count);
                        //System.Windows.Forms.MessageBox.Show(count.ToString() + "  " + length.ToString());
                        for (int i = 0; i < bdata2.Count; i++)
                        {
                            if (bdata2.ContainsKey(i) == true)
                            {
                                bdata2[i] = b.ReadBytes(length);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Loading cubemap failed because not all blocks were found. (Read)");
                    }
                }
                else
                {
                    bdata = b.ReadBytes((int)(b.BaseStream.Length - (long)128));
                }
            }
        }

        public void Write(System.IO.Stream fileStream, int cubeIndex)
        {
            using (System.IO.BinaryWriter b = new System.IO.BinaryWriter(fileStream))
            {
                b.Write(magic);
                b.Write(header.size);
                b.Write((uint)header.flags);
                b.Write(header.height);
                b.Write(header.width);
                b.Write(header.pitchOrLinearSize);
                b.Write(header.depth);
                b.Write(header.mipMapCount);
                foreach (uint u in header.reserved1)
                {
                    b.Write(u);
                }
                b.Write(header.ddspf.size);
                b.Write((uint)header.ddspf.flags);
                b.Write(header.ddspf.fourCC);
                b.Write(header.ddspf.rGBBitCount);
                b.Write(header.ddspf.rBitMask);
                b.Write(header.ddspf.gBitMask);
                b.Write(header.ddspf.bBitMask);
                b.Write(header.ddspf.aBitMask);
                b.Write((uint)header.caps);
                b.Write((uint)header.caps2);
                b.Write(header.caps3);
                b.Write(header.caps4);
                b.Write(header.reserved2);
                if (header.ddspf.flags.HasFlag(DdsPixelFormat.Flags.DDPF_FOURCC) && header.ddspf.fourCC == 808540228) // DX10
                {
                    b.Write((uint)header10.dxgiFormat);
                    b.Write((uint)header10.resourceDimension);
                    b.Write(header10.miscFlag);
                    b.Write(header10.arraySize);
                    b.Write(header10.miscFlags2);
                }
                if (cubeIndex != -1)
                {
                    b.Write(bdata2[cubeIndex]);
                }
                else if (bdata2 != null && bdata2.Count > 0)
                {
                    for (int i = 0; i < bdata2.Count; i++)
                    {
                        if (bdata2.ContainsKey(i) == true)
                        {
                            b.Write(bdata2[i]);
                        }
                    }
                }
                else
                {
                    b.Write(bdata);
                }
            }
        }
        public void Write(PssgNode node)
        {
            node.Attributes["height"].Value = header.height;
            node.Attributes["width"].Value = header.width;
            if (node.HasAttribute("numberMipMapLevels") == true)
            {
                if ((int)header.mipMapCount - 1 >= 0)
                {
                    node.Attributes["numberMipMapLevels"].Value = header.mipMapCount - 1;
                }
                else
                {
                    node.Attributes["numberMipMapLevels"].Value = 0u;
                }
            }
            if (header.ddspf.rGBBitCount == 32)
            {
                node.Attributes["texelFormat"].Value = "ui8x4";
            }
            else if (header.ddspf.rGBBitCount == 8)
            {
                node.Attributes["texelFormat"].Value = "u8";
            }
            else if (header.ddspf.fourCC == 808540228) //DX10
            {
                if (header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_TYPELESS ||
                    header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_UNORM)
                {
                    node.Attributes["texelFormat"].Value = "BC7";
                }
                else if (header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB)
                {
                    node.Attributes["texelFormat"].Value = "BC7_srgb";
                }
                else if (header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_TYPELESS ||
                    header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM)
                {
                    node.Attributes["texelFormat"].Value = "dxt1";
                }
                else if (header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB)
                {
                    node.Attributes["texelFormat"].Value = "dxt1_srgb";
                }
                else if (header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC2_TYPELESS ||
                    header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC2_UNORM)
                {
                    node.Attributes["texelFormat"].Value = "dxt3";
                }
                else if (header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC2_UNORM_SRGB)
                {
                    node.Attributes["texelFormat"].Value = "dxt3_srgb";
                }
                else if (header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_TYPELESS ||
                    header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM)
                {
                    node.Attributes["texelFormat"].Value = "dxt5";
                }
                else if (header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB)
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
                node.Attributes["texelFormat"].Value = Encoding.UTF8.GetString(BitConverter.GetBytes(header.ddspf.fourCC)).ToLower();
            }
            List<PssgNode> textureImageBlocks = node.FindNodes("TEXTUREIMAGEBLOCK");
            if (bdata2 != null && bdata2.Count > 0)
            {
                for (int i = 0; i < textureImageBlocks.Count; i++)
                {
                    switch (textureImageBlocks[i].Attributes["typename"].ToString())
                    {
                        case "Raw":
                            if (bdata2.ContainsKey(0) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = bdata2[0];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)bdata2[0].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                        case "RawNegativeX":
                            if (bdata2.ContainsKey(1) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = bdata2[1];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)bdata2[1].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                        case "RawPositiveY":
                            if (bdata2.ContainsKey(2) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = bdata2[2];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)bdata2[2].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                        case "RawNegativeY":
                            if (bdata2.ContainsKey(3) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = bdata2[3];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)bdata2[3].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                        case "RawPositiveZ":
                            if (bdata2.ContainsKey(4) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = bdata2[4];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)bdata2[4].Length;
                            }
                            else
                            {
                                throw new Exception("Loading cubemap failed because not all blocks were found. (Write)");
                            }
                            break;
                        case "RawNegativeZ":
                            if (bdata2.ContainsKey(5) == true)
                            {
                                textureImageBlocks[i].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = bdata2[5];
                                textureImageBlocks[i].Attributes["size"].Value = (UInt32)bdata2[5].Length;
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
                textureImageBlocks[0].FindNodes("TEXTUREIMAGEBLOCKDATA")[0].Value = bdata;
                textureImageBlocks[0].Attributes["size"].Value = (UInt32)bdata.Length;
            }
        }

        public UInt32 GetLinearSize()
        {
            UInt32 linearSize;

            switch (header.ddspf.fourCC)
            {
                case 0: // RGBA
                case 808540228 when header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_TYPELESS ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_UNORM ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_SNORM ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_UINT ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_SINT:
                    linearSize = (header.width * header.height) * 4;
                    break;
                case 894720068: // DXT5 aka DXGI_FORMAT_BC3_UNORM
                case 843666497: // ATI2 aka DXGI_FORMAT_BC5_UNORM
                case 808540228 when header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_TYPELESS ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC5_TYPELESS ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC5_UNORM ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC5_SNORM ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC6H_TYPELESS ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC6H_UF16 ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC6H_SF16 ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_TYPELESS ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_UNORM ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB:
                    linearSize = (header.width * header.height);
                    break;
                case 827611204: // DXT1 aka DXGI_FORMAT_BC1_UNORM
                case 826889281: // ATI1
                case 808540228 when header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_TYPELESS ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC4_TYPELESS ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC4_UNORM ||
                        header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC4_SNORM:
                    linearSize = (header.width * header.height) / 2;
                    break;
                default:
                    throw new NotImplementedException($"{nameof(GetLinearSize)} has no handler for image format {this.header.ddspf.fourCC} {(this.header.ddspf.fourCC == 808540228 ? header10.dxgiFormat.ToString() : string.Empty)}");
            }

            return linearSize;
        }
    }
}
