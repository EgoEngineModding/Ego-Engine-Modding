using System.Globalization;
using System.Numerics;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgQuatData : PkgDataList<Quaternion>
    {
        public override string Type
        {
            get
            {
                return "quat";
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

        public PkgQuatData(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            var numData = ReadHeader(reader);

            for (var i = 0; i < numData; ++i)
            {
                var vec3 = new Quaternion
                {
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle(),
                    Z = reader.ReadSingle(),
                    W = reader.ReadSingle()
                };
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
                writer.Write(val.W);
            }
        }

        public override string GetData(int index)
        {
            var vec3 = values[index];
            return Type + " " + string.Format(
                CultureInfo.InvariantCulture, "{0:0.##################},{1:0.##################},{2:0.##################},{3:0.##################}", vec3.X, vec3.Y, vec3.Z, vec3.W);
        }
        public override int SetData(string data)
        {
            var vec3s = data.Split(',');
            var res = new Quaternion(
                float.Parse(vec3s[0], CultureInfo.InvariantCulture), float.Parse(vec3s[1], CultureInfo.InvariantCulture),
                float.Parse(vec3s[2], CultureInfo.InvariantCulture), float.Parse(vec3s[3], CultureInfo.InvariantCulture));
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
