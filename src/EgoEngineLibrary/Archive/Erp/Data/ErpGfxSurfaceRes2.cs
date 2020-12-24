using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Archive.Erp.Data
{
    public struct ErpGfxSurfaceRes2Mips
    {
        public ErpCompressionAlgorithm Compression { get; set; }
        public UInt64 Offset { get; set; }
        public UInt64 PackedSize { get; set; }
        public UInt64 Size { get; set; }
    }

    public class ErpGfxSurfaceRes2 : ErpFragmentData
    {
        private bool hasTwoUnknowns;
        public string MipMapFileName { get; set; }
        public List<ErpGfxSurfaceRes2Mips> Mips { get; set; }
        public float Unknown { get; set; }
        public float Unknown2 { get; set; }

        public ErpGfxSurfaceRes2()
        {
            MipMapFileName = string.Empty;
            Mips = new List<ErpGfxSurfaceRes2Mips>();
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
                    mip.Compression = (ErpCompressionAlgorithm)reader.ReadByte();
                    mip.Offset = reader.ReadUInt64();
                    mip.PackedSize = reader.ReadUInt64();
                    mip.Size = reader.ReadUInt64();
                    Mips.Add(mip);
                }

                long leftoverBytes = reader.BaseStream.Length - reader.BaseStream.Position;
                if (leftoverBytes == 8)
                {
                    // This part was introduced in F1 2017
                    hasTwoUnknowns = true;
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

                if (hasTwoUnknowns)
                {
                    writer.Write(Unknown);
                    writer.Write(Unknown2);
                }

                fragment.SetData(newData.ToArray());
            }
        }
    }
}
