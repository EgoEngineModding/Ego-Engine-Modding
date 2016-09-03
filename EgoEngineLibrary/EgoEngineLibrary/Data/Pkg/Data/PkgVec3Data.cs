using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgVec3Data : PkgDataList<Vector3>
    {
        public override string Type
        {
            get
            {
                return "vec3";
            }
        }

        protected override uint DataByteSize
        {
            get
            {
                return 16;
            }
        }

        public override int Align
        {
            get
            {
                return 16;
            }
        }

        public PkgVec3Data(PkgFile parentFile)
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
                values.Add(vec3);
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (Vector3 val in values)
            {
                writer.Write(val.X);
                writer.Write(val.Y);
                writer.Write(val.Z);
                writer.Write((UInt32)0);
            }
        }

        public override string GetData(Int32 index)
        {
            Vector3 vec3 = values[index];
            return Type + " " + string.Format(
                CultureInfo.InvariantCulture, "{0:0.##################},{1:0.##################},{2:0.##################}", vec3.X, vec3.Y, vec3.Z);
        }
        public override Int32 SetData(string data)
        {
            string[] vec3s = data.Split(',');
            Vector3 res = new Vector3(
                float.Parse(vec3s[0], CultureInfo.InvariantCulture), float.Parse(vec3s[1], CultureInfo.InvariantCulture), float.Parse(vec3s[2], CultureInfo.InvariantCulture));
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
