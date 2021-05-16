using System.Collections.Generic;
using System.IO;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgStringData : PkgData
    {
        private readonly Dictionary<string, int> strgOffset;
        private MemoryStream strgData;

        protected override string ChunkType
        {
            get
            {
                return "!sbi";
            }
        }

        public override string Type
        {
            get
            {
                return "stri";
            }
        }

        public override int Align
        {
            get
            {
                return 4;
            }
        }

        public PkgStringData(PkgFile parentFile)
            : base(parentFile)
        {
            strgOffset = new Dictionary<string, int>();
            strgData = new MemoryStream();
        }

        ~PkgStringData()
        {
            // Yikes what was I thinking when I wrote this
            // this needs refactoring, shouldn't be using finalizer
            strgData.Dispose();
        }

        public override void Read(PkgBinaryReader reader)
        {
            var dataLength = reader.ReadUInt32();
            strgData = new MemoryStream(reader.ReadBytes((int)dataLength));
        }
        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(new byte[(-(int)writer.BaseStream.Position) & 3]);
            writer.Write("!sbi", 4);
            writer.Write((uint)strgData.Length);
            writer.Write(strgData.ToArray());
        }
        internal override void UpdateOffsets()
        {
            PkgValue._offset += 8 + (int)strgData.Length;
        }

        public override string GetData(int index)
        {
            var reader = new PkgBinaryReader(strgData);
            reader.Seek(index, SeekOrigin.Begin);
            return Type + " " + reader.ReadString();
        }
        public override int SetData(string data)
        {
            int index;
            if (strgOffset.TryGetValue(data, out index))
            {
                return index;
            }
            else
            {
                var writer = new PkgBinaryWriter(strgData);
                index = (int)strgData.Length;
                writer.Write(data);
                strgOffset.Add(data, index);
                return index;
            }
        }
    }
}
