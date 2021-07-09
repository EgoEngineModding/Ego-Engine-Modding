using Newtonsoft.Json;
using System;
using System.IO;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgObject : PkgArray<PkgPairBase>
    {
        private const string VersionPropertyJson = "PkgObjectVersion";

        protected override string ChunkType
        {
            get
            {
                return "!idi";
            }
        }

        // V1 added in F1 2021
        public byte Version { get; private set; }

        public PkgObject(PkgFile parentFile)
            : base(parentFile, x => new PkgPair(x))
        {
            Version = 1;
        }

        public override void Read(PkgBinaryReader reader)
        {
            var elemVerData = reader.ReadOffsetType();
            var numElements = elemVerData.Offset;
            Version = elemVerData.Type;

            for (var i = 0; i < numElements; ++i)
            {
                PkgPairBase val;
                if (Version == 0)
                {
                    val = new PkgPair(ParentFile);
                }
                else
                {
                    val = new PkgPairV1(ParentFile);
                }

                Elements.Add(val);
                val.Read(reader);
            }
        }

        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(ChunkType, 4);
            var elemVerData = new PkgOffsetType()
            {
                Offset = Elements.Count,
                Type = Version
            };
            writer.Write(elemVerData);

            foreach (var val in Elements)
            {
                val.Write(writer);
            }

            foreach (var val in Elements)
            {
                val.WriteComplexValue(writer);
            }
        }

        internal override void UpdateOffsets()
        {
            var elementLength = Version == 0 ? 8 : 12;
            PkgValue._offset += 8 + elementLength * Elements.Count;

            foreach (var pair in Elements)
            {
                pair.UpdateOffsets();
            }
        }

        public override void FromJson(JsonTextReader reader)
        {
            // Read out special version prop, and pick PkgPair based on that
            Version = ReadJsonVersionProperty(reader);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }

                PkgPairBase pair = Version switch
                {
                    0 => new PkgPair(ParentFile),
                    1 => new PkgPairV1(ParentFile),
                    _ => throw new InvalidDataException($"Unexpected PkgObject version ({Version}).")
                };

                Elements.Add(pair);
                pair.FromJson(reader);
            }

            static byte ReadJsonVersionProperty(JsonTextReader reader)
            {
                var error = $"Expected object to have {VersionPropertyJson} as first property.";

                reader.Read();
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new InvalidDataException(error);

                var propName = (string?)reader.Value ?? string.Empty;
                if (!VersionPropertyJson.Equals(propName, StringComparison.InvariantCulture))
                    throw new InvalidDataException(error);

                var version = reader.ReadAsInt32();
                if (version is null)
                    throw new InvalidDataException(error);

                return Convert.ToByte(version);
            }
        }

        public override void ToJson(JsonTextWriter writer)
        {
            writer.WriteStartObject();

            // Write out special version prop
            writer.WritePropertyName(VersionPropertyJson);
            writer.WriteValue(Version);

            foreach (var pair in Elements)
            {
                pair.ToJson(writer);
            }

            writer.WriteEndObject();
        }
    }
}
