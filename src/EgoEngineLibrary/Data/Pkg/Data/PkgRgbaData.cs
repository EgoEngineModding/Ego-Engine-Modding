namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgRgbaData : PkgDataList<uint>
    {
        public override string Type
        {
            get
            {
                return "rgba";
            }
        }

        protected override uint DataByteSize
        {
            get
            {
                return 4;
            }
        }

        public override int Align
        {
            get
            {
                return 4;
            }
        }

        public PkgRgbaData(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            var numData = ReadHeader(reader);

            for (var i = 0; i < numData; ++i)
            {
                values.Add(reader.ReadUInt32());
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (var val in values)
            {
                writer.Write(val);
            }
        }

        public override string GetData(int index)
        {
            return Type + " " + values[index].ToString("X");
        }
        public override int SetData(string data)
        {
            var res = uint.Parse(data, System.Globalization.NumberStyles.HexNumber);
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
