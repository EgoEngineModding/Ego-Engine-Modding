using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgMat4Data : PkgDataList<Matrix4x4>
    {
        public override string Type
        {
            get
            {
                return "mat4";
            }
        }

        protected override uint DataByteSize
        {
            get
            {
                return 64;
            }
        }

        public override int Align
        {
            get
            {
                return 64;
            }
        }

        public PkgMat4Data(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            UInt32 numData = ReadHeader(reader);

            for (int i = 0; i < numData; ++i)
            {
                Matrix4x4 mat4 = new Matrix4x4();
                mat4.M11 = reader.ReadSingle();
                mat4.M12 = reader.ReadSingle();
                mat4.M13 = reader.ReadSingle();
                mat4.M14 = reader.ReadSingle();
                mat4.M21 = reader.ReadSingle();
                mat4.M22 = reader.ReadSingle();
                mat4.M23 = reader.ReadSingle();
                mat4.M24 = reader.ReadSingle();
                mat4.M31 = reader.ReadSingle();
                mat4.M32 = reader.ReadSingle();
                mat4.M33 = reader.ReadSingle();
                mat4.M34 = reader.ReadSingle();
                mat4.M41 = reader.ReadSingle();
                mat4.M42 = reader.ReadSingle();
                mat4.M43 = reader.ReadSingle();
                mat4.M44 = reader.ReadSingle();
                values.Add(mat4);
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (Matrix4x4 m4 in values)
            {
                writer.Write(m4.M11); writer.Write(m4.M12); writer.Write(m4.M13); writer.Write(m4.M14);
                writer.Write(m4.M21); writer.Write(m4.M22); writer.Write(m4.M23); writer.Write(m4.M24);
                writer.Write(m4.M31); writer.Write(m4.M32); writer.Write(m4.M33); writer.Write(m4.M34);
                writer.Write(m4.M41); writer.Write(m4.M42); writer.Write(m4.M43); writer.Write(m4.M44);
            }
        }

        public override string GetData(Int32 index)
        {
            Matrix4x4 m = values[index];
            return Type + " " + string.Format(CultureInfo.InvariantCulture, 
                "{0:0.##################},{1:0.##################},{2:0.##################},{3:0.##################};{4:0.##################},{5:0.##################},{6:0.##################},{7:0.##################};{8:0.##################},{9:0.##################},{10:0.##################},{11:0.##################};{12:0.##################},{13:0.##################},{14:0.##################},{15:0.##################}",
                m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
        }
        public override Int32 SetData(string data)
        {
            string[] s = data.Split(',', ';');
            Matrix4x4 m = new Matrix4x4(
                float.Parse(s[0], CultureInfo.InvariantCulture), float.Parse(s[1], CultureInfo.InvariantCulture), float.Parse(s[2], CultureInfo.InvariantCulture), float.Parse(s[3], CultureInfo.InvariantCulture),
                float.Parse(s[4], CultureInfo.InvariantCulture), float.Parse(s[5], CultureInfo.InvariantCulture), float.Parse(s[6], CultureInfo.InvariantCulture), float.Parse(s[7], CultureInfo.InvariantCulture),
                float.Parse(s[8], CultureInfo.InvariantCulture), float.Parse(s[9], CultureInfo.InvariantCulture), float.Parse(s[10], CultureInfo.InvariantCulture), float.Parse(s[11], CultureInfo.InvariantCulture),
                float.Parse(s[12], CultureInfo.InvariantCulture), float.Parse(s[13], CultureInfo.InvariantCulture), float.Parse(s[14], CultureInfo.InvariantCulture), float.Parse(s[15], CultureInfo.InvariantCulture));
            int index = values.IndexOf(m);

            if (index >= 0)
            {
                return index;
            }
            else
            {
                index = values.Count;
                values.Add(m);
                return index;
            }
        }
    }
}
