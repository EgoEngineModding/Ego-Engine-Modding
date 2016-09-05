using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgDataArrayReference : PkgComplexValue
    {
        protected override string ChunkType
        {
            get
            {
                return "!iar";
            }
        }

        string reference;

        public PkgDataArrayReference(PkgFile parentFile)
            : base(parentFile)
        {
        }
        
        public override void Read(PkgBinaryReader reader)
        {
            reference = reader.ReadString(8);
        }
        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(ChunkType, 4);
            writer.Write(reference, 8);
        }

        internal override void UpdateOffsets()
        {
            PkgValue._offset += 12;
        }

        public override void FromJson(JsonTextReader reader)
        {
            reference = ((string)reader.Value).Substring(5);
        }
        public override void ToJson(JsonTextWriter writer)
        {
            writer.WriteValue(ChunkType + " " + reference);
        }
    }
}
