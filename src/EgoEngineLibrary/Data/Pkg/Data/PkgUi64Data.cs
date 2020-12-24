using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgUi64Data : PkgDataList<UInt64>
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
            UInt32 numData = ReadHeader(reader);

            for (int i = 0; i < numData; ++i)
            {
                values.Add(reader.ReadUInt64());
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (UInt64 val in values)
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
            UInt64 res = UInt64.Parse(data);
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
