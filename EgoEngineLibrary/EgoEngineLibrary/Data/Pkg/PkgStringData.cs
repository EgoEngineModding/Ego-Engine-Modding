using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgStringData : PkgData
    {
        readonly Dictionary<string, int> strgOffset;
        MemoryStream strgData;

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

        public PkgStringData(PkgFile parentFile)
            : base(parentFile)
        {
            strgOffset = new Dictionary<string, int>();
            strgData = new MemoryStream();
        }

        ~PkgStringData()
        {
            strgData.Dispose();
        }

        public override void Read(PkgBinaryReader reader)
        {
            UInt32 dataLength = reader.ReadUInt32();
            strgData = new MemoryStream(reader.ReadBytes((int)dataLength));
        }
        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(new byte[(-(Int32)writer.BaseStream.Position) & 3]);
            writer.Write("!sbi", 4);
            writer.Write((UInt32)strgData.Length);
            writer.Write(strgData.ToArray());
        }
        internal override void UpdateOffsets()
        {
            PkgValue._offset += 8 + (Int32)strgData.Length;
        }

        public override string GetData(PkgOffsetType offsetType)
        {
            PkgBinaryReader reader = new PkgBinaryReader(strgData);
            reader.Seek(offsetType.Offset, SeekOrigin.Begin);
            return Type + " " + reader.ReadString();
        }
        public override void SetData(string data, PkgOffsetType offsetType)
        {
            string type = data.Remove(4);
            data = data.Substring(5);

            int index;
            if (strgOffset.TryGetValue(data, out index))
            {
                offsetType.Offset = index;
            }
            else
            {
                PkgBinaryWriter writer = new PkgBinaryWriter(strgData);
                offsetType.Offset = (int)strgData.Length;
                writer.Write(data);
                strgOffset.Add(data, offsetType.Offset);
            }
        }

        public override void FromJson(JsonTextReader reader)
        {
            throw new NotImplementedException();
        }
        public override void ToJson(JsonTextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
