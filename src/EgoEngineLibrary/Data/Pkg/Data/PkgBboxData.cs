using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgBboxData : PkgDataList<(Vector3, Vector3)>
    {
        public override string Type
        {
            get
            {
                return "bbox";
            }
        }

        protected override uint DataByteSize
        {
            get
            {
                return 32;
            }
        }

        public override int Align
        {
            get
            {
                return 32;
            }
        }

        public PkgBboxData(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            UInt32 numData = ReadHeader(reader);

            for (int i = 0; i < numData; ++i)
            {
                Vector3 vec3 = new Vector3();
                vec3.X = reader.ReadSingle();
                vec3.Y = reader.ReadSingle();
                vec3.Z = reader.ReadSingle();
                reader.Seek(4, SeekOrigin.Current);
                Vector3 vec32 = new Vector3();
                vec32.X = reader.ReadSingle();
                vec32.Y = reader.ReadSingle();
                vec32.Z = reader.ReadSingle();
                reader.Seek(4, SeekOrigin.Current);
                values.Add((vec3, vec32));
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach ((Vector3,Vector3) val in values)
            {
                writer.Write(val.Item1.X);
                writer.Write(val.Item1.Y);
                writer.Write(val.Item1.Z);
                writer.Write((UInt32)0);
                writer.Write(val.Item2.X);
                writer.Write(val.Item2.Y);
                writer.Write(val.Item2.Z);
                writer.Write((UInt32)0);
            }
        }

        public override string GetData(Int32 index)
        {
            (Vector3,Vector3) val = values[index];
            return Type + " " + string.Format(
                CultureInfo.InvariantCulture, $"{val.Item1.X:0.##################},{val.Item1.Y:0.##################},{val.Item1.Z:0.##################};{val.Item2.X:0.##################},{val.Item2.Y:0.##################},{val.Item2.Z:0.##################}");
        }
        public override Int32 SetData(string data)
        {
            string[] vec3s = data.Split(',', ';');
            Vector3 vec1 = new Vector3(
                float.Parse(vec3s[0], CultureInfo.InvariantCulture), float.Parse(vec3s[1], CultureInfo.InvariantCulture), float.Parse(vec3s[2], CultureInfo.InvariantCulture));
            Vector3 vec2 = new Vector3(
                float.Parse(vec3s[3], CultureInfo.InvariantCulture), float.Parse(vec3s[4], CultureInfo.InvariantCulture), float.Parse(vec3s[5], CultureInfo.InvariantCulture));
            (Vector3, Vector3) res = (vec1, vec2);
            int index = values.IndexOf(res);

            if (index >= 0)
            {
                return index;
            }
            else
            {
                index = values.Count;
                values.Add(res);
                return index;
            }
        }
    }
}
