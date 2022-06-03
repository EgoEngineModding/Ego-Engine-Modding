using System;
using System.Collections.Generic;
using System.IO;

namespace EgoEngineLibrary.Archive.Erp.Data
{
    public enum ErpGfxSurfaceResMipCompressionAlgorithm : byte
    {
        None,
        LZ4 = 0x03,
        ZStandard = 0x04,
    }
    
    public struct ErpGfxSurfaceRes2Mips
    {
        public ErpGfxSurfaceResMipCompressionAlgorithm Compression { get; set; }
        public UInt64 Offset { get; set; }
        public UInt64 PackedSize { get; set; }
        public UInt64 Size { get; set; }
    }

    public class ErpGfxSurfaceRes2 : ErpFragmentData
    {
        private bool _hasTwoUnknowns;
        private bool _hasGridLegUnknown;

        public string MipMapFileName { get; set; }
        public List<ErpGfxSurfaceRes2Mips> Mips { get; set; }
        public float Unknown { get; set; }
        public float Unknown2 { get; set; }
        public byte Unknown3 { get; set; }

        public ErpGfxSurfaceRes2()
        {
            MipMapFileName = string.Empty;
            Mips = new List<ErpGfxSurfaceRes2Mips>();
            Unknown3 = 0;
            Unknown = 25;
            Unknown2 = 1;
        }

        public override void FromFragment(ErpFragment fragment)
        {
            using (var memData = fragment.GetDataStream(true))
            using (ErpBinaryReader reader = new ErpBinaryReader(memData))
            {
                MipMapFileName = reader.ReadString(reader.ReadByte());
                UInt32 mipMapCount = reader.ReadUInt32();

                Mips = new List<ErpGfxSurfaceRes2Mips>((int)mipMapCount);
                for (int i = 0; i < mipMapCount; ++i)
                {
                    ErpGfxSurfaceRes2Mips mip = new ErpGfxSurfaceRes2Mips();
                    mip.Compression = (ErpGfxSurfaceResMipCompressionAlgorithm)reader.ReadByte();
                    mip.Offset = reader.ReadUInt64();
                    mip.PackedSize = reader.ReadUInt64();
                    mip.Size = reader.ReadUInt64();
                    Mips.Add(mip);
                }

                long leftoverBytes = reader.BaseStream.Length - reader.BaseStream.Position;
                if (leftoverBytes == 9)
                {
                    // This part was introduced in Grid Legends
                    _hasTwoUnknowns = true;
                    _hasGridLegUnknown = true;
                    Unknown3 = reader.ReadByte();
                    Unknown = reader.ReadSingle();
                    Unknown2 = reader.ReadSingle();
                }
                else if (leftoverBytes == 8)
                {
                    // This part was introduced in F1 2017
                    _hasTwoUnknowns = true;
                    Unknown = reader.ReadSingle();
                    Unknown2 = reader.ReadSingle();
                }
                else if (leftoverBytes > 0)
                {
                    throw new NotSupportedException("The GfxSurfaceRes2 data is not supported.");
                }
            }
        }

        public override void ToFragment(ErpFragment fragment)
        {
            using (var newData = new MemoryStream())
            using (ErpBinaryWriter writer = new ErpBinaryWriter(MiscUtil.Conversion.EndianBitConverter.Little, newData))
            {
                writer.Write((byte)MipMapFileName.Length);
                writer.Write(MipMapFileName, MipMapFileName.Length);
                writer.Write((UInt32)Mips.Count);

                for (int i = 0; i < Mips.Count; ++i)
                {
                    writer.Write((byte)Mips[i].Compression);
                    writer.Write(Mips[i].Offset);
                    writer.Write(Mips[i].PackedSize);
                    writer.Write(Mips[i].Size);
                }

                if (_hasGridLegUnknown)
                {
                    writer.Write(Unknown3);
                }

                if (_hasTwoUnknowns)
                {
                    writer.Write(Unknown);
                    writer.Write(Unknown2);
                }

                fragment.SetData(newData.ToArray());
            }
        }
    }
}
