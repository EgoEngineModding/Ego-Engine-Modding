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
        public Dictionary<int, byte[]> bdata2;

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
            bdata = Array.Empty<byte>();
            bdata2 = new Dictionary<int, byte[]>();
        }
        public DdsFile(System.IO.Stream fileStream)
            : this()
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
                        bdata2.Add(0, Array.Empty<byte>());
                    }
                    else
                    {
                        bdata2.Add(-1, Array.Empty<byte>());
                    }
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEX) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEX)
                    {
                        count++;
                        bdata2.Add(1, Array.Empty<byte>());
                    }
                    else
                    {
                        bdata2.Add(-2, Array.Empty<byte>());
                    }
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEY) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEY)
                    {
                        count++;
                        bdata2.Add(2, Array.Empty<byte>());
                    }
                    else
                    {
                        bdata2.Add(-3, Array.Empty<byte>());
                    }
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEY) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEY)
                    {
                        count++;
                        bdata2.Add(3, Array.Empty<byte>());
                    }
                    else
                    {
                        bdata2.Add(-4, Array.Empty<byte>());
                    }
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEZ) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_POSITIVEZ)
                    {
                        count++;
                        bdata2.Add(4, Array.Empty<byte>());
                    }
                    else
                    {
                        bdata2.Add(-5, Array.Empty<byte>());
                    }
                    if ((header.caps2 & DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEZ) == DdsHeader.Caps2.DDSCAPS2_CUBEMAP_NEGATIVEZ)
                    {
                        count++;
                        bdata2.Add(5, Array.Empty<byte>());
                    }
                    else
                    {
                        bdata2.Add(-6, Array.Empty<byte>());
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
            using (System.IO.BinaryWriter b = new System.IO.BinaryWriter(fileStream, Encoding.UTF8, true))
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
                case 1429553986: // BC5U from Intel® Texture Works Plugin for Photoshop
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
                case 1429488450: // BC4U from Intel® Texture Works Plugin for Photoshop
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
