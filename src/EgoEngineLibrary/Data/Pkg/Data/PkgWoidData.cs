using System;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgWoidData : PkgDataList<string>
    {
        public override string Type
        {
            get
            {
                return "woid";
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

        public PkgWoidData(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            var numData = ReadHeader(reader);

            for (var i = 0; i < numData; ++i)
            {
                values.Add(Convert.ToBase64String(reader.ReadBytes(16), Base64FormattingOptions.None));
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (var val in values)
            {
                writer.Write(Convert.FromBase64String(val));
            }
        }

        public override string GetData(int index)
        {
            return Type + " " + values[index];
        }
        public override int SetData(string data)
        {
            var index = values.IndexOf(data);

            if (index >= 0)
            {
                return index;
            }
            else
            {
                index = values.Count;
                values.Add(data);
                return index;
            }
        }
    }
}
