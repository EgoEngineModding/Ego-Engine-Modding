using System.Text.Json;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgObject : PkgArray<PkgPairBase>
    {
        private static ReadOnlySpan<byte> VersionPropertyJson => "PkgObjectVersion"u8;

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

        public override void FromJson(ref Utf8JsonReader reader)
        {
            // Read out special version prop, and pick PkgPair based on that
            var versionReader = reader;
            if (TryReadJsonVersionProperty(ref versionReader, out var version))
            {
                reader = versionReader;
            }

            Version = version;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
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
                pair.FromJson(ref reader);
            }

            static bool TryReadJsonVersionProperty(ref Utf8JsonReader reader, out byte version)
            {
                version = 0;
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                    return false;

                if (!reader.ValueTextEquals(VersionPropertyJson))
                    return false;

                reader.Read();
                return reader.TryGetByte(out version);
            }
        }

        public override void ToJson(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            // Write out special version prop
            if (Version != 0)
            {
                writer.WriteNumber(VersionPropertyJson, Version);
            }

            foreach (var pair in Elements)
            {
                pair.ToJson(writer);
            }

            writer.WriteEndObject();
        }
    }
}
