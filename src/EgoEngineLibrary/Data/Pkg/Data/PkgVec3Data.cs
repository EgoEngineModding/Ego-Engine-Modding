using System.Globalization;
using System.IO;
using System.Numerics;

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
            var numData = ReadHeader(reader);

            for (var i = 0; i < numData; ++i)
            {
                var vec3 = new Vector3
                {
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle(),
                    Z = reader.ReadSingle()
                };
                reader.Seek(4, SeekOrigin.Current);
                values.Add(vec3);
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (var val in values)
            {
                writer.Write(val.X);
                writer.Write(val.Y);
                writer.Write(val.Z);
                writer.Write((uint)0);
            }
        }

        public override string GetData(int index)
        {
            var vec3 = values[index];
            return Type + " " + string.Format(
                CultureInfo.InvariantCulture, "{0:0.##################},{1:0.##################},{2:0.##################}", vec3.X, vec3.Y, vec3.Z);
        }
        public override int SetData(string data)
        {
            var vec3s = data.Split(',');
            var res = new Vector3(
                float.Parse(vec3s[0], CultureInfo.InvariantCulture), float.Parse(vec3s[1], CultureInfo.InvariantCulture), float.Parse(vec3s[2], CultureInfo.InvariantCulture));
            var index = values.IndexOf(res);

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
