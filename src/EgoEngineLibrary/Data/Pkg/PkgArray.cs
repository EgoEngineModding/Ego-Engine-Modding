using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EgoEngineLibrary.Data.Pkg
{
    public abstract class PkgArray<T> : PkgComplexValue
        where T : PkgValue
    {
        readonly List<T> elements;
        readonly Func<PkgFile, T> elementFactory;

        public List<T> Elements
        {
            get
            {
                return elements;
            }
        }

        public PkgArray(PkgFile parentFile, Func<PkgFile, T> factory)
            : base(parentFile)
        {
            elements = new List<T>();
            elementFactory = factory;
        }

        public override void Read(PkgBinaryReader reader)
        {
            UInt32 numElements = reader.ReadUInt32();

            for (int i = 0; i < numElements; ++i)
            {
                T val = elementFactory(ParentFile);
                Elements.Add(val);
                val.Read(reader);
            }
        }

        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(ChunkType, 4);
            writer.Write((UInt32)Elements.Count);

            foreach (T val in Elements)
            {
                val.Write(writer);
            }

            foreach (T val in Elements)
            {
                val.WriteComplexValue(writer);
            }
        }
    }

    public class PkgArray : PkgArray<PkgValue>
    {
        protected override string ChunkType
        {
            get
            {
                return "!ili";
            }
        }

        public PkgArray(PkgFile parentFile)
            : base(parentFile, x => new PkgValue(x) )
        {
        }

        internal override void UpdateOffsets()
        {
            PkgValue._offset += 8 + 4 * Elements.Count;

            foreach (PkgValue val in Elements)
            {
                val.UpdateOffsets();
            }
        }

        public override void FromJson(JsonTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }
                
                PkgValue val = new PkgValue(ParentFile);
                Elements.Add(val);
                val.FromJson(reader);
            }
        }
        public override void ToJson(JsonTextWriter writer)
        {
            writer.WriteStartArray();

            foreach (PkgValue val in Elements)
            {
                val.ToJson(writer);
            }

            writer.WriteEndArray();
        }
    }
}
