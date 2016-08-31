using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgObject : PkgArray<PkgPair>
    {
        protected override string ChunkType
        {
            get
            {
                return "!idi";
            }
        }

        public PkgObject(PkgFile parentFile)
            : base(parentFile, x => new PkgPair(parentFile))
        {
        }

        internal override void UpdateOffsets()
        {
            PkgValue._offset += 8 + 8 * Elements.Count;

            foreach (PkgPair pair in Elements)
            {
                pair.UpdateOffsets();
            }
        }

        public override void FromJson(JsonTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }

                PkgPair pair = new PkgPair(ParentFile);
                Elements.Add(pair);
                pair.FromJson(reader);
            }
        }
        public override void ToJson(JsonTextWriter writer)
        {
            writer.WriteStartObject();

            foreach (PkgPair pair in Elements)
            {
                pair.ToJson(writer);
            }

            writer.WriteEndObject();
        }
    }
}
