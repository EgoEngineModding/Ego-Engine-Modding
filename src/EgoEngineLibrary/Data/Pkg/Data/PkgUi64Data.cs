namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgUi64Data : PkgDataList<ulong>
    {
        public override string Type
        {
            get
            {
                return "ui64";
            }
        }

        protected override uint DataByteSize
        {
            get
            {
                return 8;
            }
        }

        public override int Align
        {
            get
            {
                return 8;
            }
        }

        public PkgUi64Data(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            var numData = ReadHeader(reader);

            for (var i = 0; i < numData; ++i)
            {
                values.Add(reader.ReadUInt64());
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
            return Type + " " + values[index];
        }
        public override int SetData(string data)
        {
            var res = ulong.Parse(data);
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
