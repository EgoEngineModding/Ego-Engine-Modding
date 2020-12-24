using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgVec4Data : PkgDataList<Vector4>
    {
        public override string Type
        {
            get
            {
                return "vec4";
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

        public PkgVec4Data(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            UInt32 numData = ReadHeader(reader);

            for (int i = 0; i < numData; ++i)
            {
                Vector4 vec3 = new Vector4();
                vec3.X = reader.ReadSingle();
                vec3.Y = reader.ReadSingle();
                vec3.Z = reader.ReadSingle();
                vec3.W = reader.ReadSingle();
                values.Add(vec3);
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (Vector4 val in values)
            {
                writer.Write(val.X);
                writer.Write(val.Y);
                writer.Write(val.Z);
                writer.Write(val.W);
            }
        }

        public override string GetData(Int32 index)
        {
            Vector4 vec3 = values[index];
            return Type + " " + string.Format(CultureInfo.InvariantCulture, "{0:0.##################},{1:0.##################},{2:0.##################},{3:0.##################}", vec3.X, vec3.Y, vec3.Z, vec3.W);
        }
        public override Int32 SetData(string data)
        {
            string[] vec3s = data.Split(',');
            Vector4 res = new Vector4(
                float.Parse(vec3s[0], CultureInfo.InvariantCulture), float.Parse(vec3s[1], CultureInfo.InvariantCulture), 
                float.Parse(vec3s[2], CultureInfo.InvariantCulture), float.Parse(vec3s[3], CultureInfo.InvariantCulture));
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
