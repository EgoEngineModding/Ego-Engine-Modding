using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Archive.Erp.Data
{
    public class ErpGfxSurfaceRes0 : ErpFragmentData
    {
        public Int32 Unknown { get; set; }
        public Int32 Unknown2 { get; set; }
        public ErpGfxSurfaceFormat ImageType { get; set; }
        public UInt32 Width { get; set; }
        public UInt32 Height { get; set; }
        public Int32 Unknown3 { get; set; }
        public UInt32 MipMapCount { get; set; }
        public UInt32 ArraySize { get; set; }
        public Int32 Unknown4 { get; set; }

        public ErpGfxSurfaceRes0()
        {
            Unknown = 2;
            Unknown2 = 264;
            ImageType = ErpGfxSurfaceFormat.DXT5_2;
            Width = 16;
            Height = 16;
            Unknown3 = 1;
            MipMapCount = 1;
            ArraySize = 1;
            Unknown4 = 0;
        }

        public override void FromFragment(ErpFragment fragment)
        {
            using (var memData = fragment.GetDataStream(true))
            using (ErpBinaryReader reader = new ErpBinaryReader(memData))
            {
                Unknown = reader.ReadInt32();
                Unknown2 = reader.ReadInt32();
                ImageType = (ErpGfxSurfaceFormat)reader.ReadInt32();
                Width = reader.ReadUInt32();
                Height = reader.ReadUInt32();
                Unknown3 = reader.ReadInt32();
                MipMapCount = reader.ReadUInt32();
                ArraySize = reader.ReadUInt32();
                Unknown4 = reader.ReadInt32();
            }
        }

        public override void ToFragment(ErpFragment fragment)
        {
            using (var newData = new MemoryStream())
            using (ErpBinaryWriter writer = new ErpBinaryWriter(MiscUtil.Conversion.EndianBitConverter.Little, newData))
            {
                writer.Write(Unknown);
                writer.Write(Unknown2);
                writer.Write((Int32)ImageType);
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(Unknown3);
                writer.Write(MipMapCount);
                writer.Write(ArraySize);
                writer.Write(Unknown4);

                fragment.SetData(newData.ToArray());
            }
        }
    }
}
