using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public abstract class PkgDataList<T> : PkgData
    {
        protected readonly List<T> values;

        protected abstract UInt32 DataByteSize { get; }

        public PkgDataList(PkgFile parentFile)
            : base(parentFile)
        {
            values = new List<T>();
        }

        protected UInt32 ReadHeader(PkgBinaryReader reader)
        {
            UInt32 numData = reader.ReadUInt32();
            UInt32 bytesPerData = reader.ReadUInt32();
            return numData;
        }
        protected void WriteHeader(PkgBinaryWriter writer)
        {
            writer.Write(new byte[GetPaddingLength((Int32)writer.BaseStream.Position)]);
            writer.Write(ChunkType, 4);
            writer.Write(Type, 4);

            writer.Write((UInt32)values.Count);
            writer.Write(DataByteSize);
        }

        internal override void UpdateOffsets()
        {
            PkgValue._offset += 16 + values.Count * (Int32)DataByteSize;
        }
    }
}
