using System.Collections.Generic;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public abstract class PkgDataList<T> : PkgData
    {
        protected readonly List<T> values;

        protected abstract uint DataByteSize { get; }

        public PkgDataList(PkgFile parentFile)
            : base(parentFile)
        {
            values = new List<T>();
        }

        protected uint ReadHeader(PkgBinaryReader reader)
        {
            var numData = reader.ReadUInt32();
            _ = reader.ReadUInt32(); // bytesPerData
            return numData;
        }
        protected void WriteHeader(PkgBinaryWriter writer)
        {
            writer.Write(new byte[GetPaddingLength((int)writer.BaseStream.Position)]);
            writer.Write(ChunkType, 4);
            writer.Write(Type, 4);

            writer.Write((uint)values.Count);
            writer.Write(DataByteSize);
        }

        internal override void UpdateOffsets()
        {
            PkgValue._offset += 16 + values.Count * (int)DataByteSize;
        }
    }
}
