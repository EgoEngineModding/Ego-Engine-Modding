using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EgoEngineLibrary.Conversion;

namespace EgoEngineLibrary.Archive.Erp.Data
{
    public class ErpGfxSRVResource0 : ErpFragmentData
    {
        public int Unknown { get; set; }
        public ErpGfxSurfaceFormat ImageType { get; set; }
        public int Unknown2 { get; set; }
        public uint MipMapCount { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
        public string SurfaceResourceName { get; set; }

        public ErpGfxSRVResource0()
        {
            Unknown = 5;
            ImageType = ErpGfxSurfaceFormat.DXT5_SRGB;
            Unknown2 = 0;
            MipMapCount = 1;
            Unknown3 = 0;
            Unknown4 = 0;
            SurfaceResourceName = string.Empty;
        }

        public override void FromFragment(ErpFragment fragment)
        {
            using var memData = fragment.GetDataStream(true);
            using var reader = new ErpBinaryReader(memData);
            Unknown = reader.ReadInt32();
            ImageType = (ErpGfxSurfaceFormat)reader.ReadInt32();
            Unknown2 = reader.ReadInt32();
            MipMapCount = reader.ReadUInt32();
            Unknown3 = reader.ReadInt32();
            Unknown4 = reader.ReadInt32();
            SurfaceResourceName = reader.ReadString();
        }

        public override void ToFragment(ErpFragment fragment)
        {
            using var newData = new MemoryStream();
            using var writer = new ErpBinaryWriter(EndianBitConverter.Little, newData);
            writer.Write(Unknown);
            writer.Write((int)ImageType);
            writer.Write(Unknown2);
            writer.Write(MipMapCount);
            writer.Write(Unknown3);
            writer.Write(Unknown4);
            writer.Write(SurfaceResourceName);

            fragment.SetData(newData.ToArray());
        }
    }
}
