namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgShnmData : PkgDataList<string>
    {
        public override string Type
        {
            get
            {
                return "shnm";
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

        public PkgShnmData(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            var numData = ReadHeader(reader);

            for (var i = 0; i < numData; ++i)
            {
                values.Add(reader.ReadString(16));
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (var val in values)
            {
                writer.Write(val, 16);
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
