namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgSi32Data : PkgDataList<int>
    {
        public override string Type
        {
            get
            {
                return "si32";
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

        public PkgSi32Data(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            var numData = ReadHeader(reader);

            for (var i = 0; i < numData; ++i)
            {
                values.Add(reader.ReadInt32());
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
            var res = int.Parse(data);
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
