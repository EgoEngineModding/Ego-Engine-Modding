using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgSi32Data : PkgDataList<Int32>
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
            UInt32 numData = ReadHeader(reader);

            for (int i = 0; i < numData; ++i)
            {
                values.Add(reader.ReadInt32());
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (Int32 val in values)
            {
                writer.Write(val);
            }
        }

        public override string GetData(Int32 index)
        {
            return Type + " " + values[index];
        }
        public override Int32 SetData(string data)
        {
            Int32 res = Int32.Parse(data);
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
