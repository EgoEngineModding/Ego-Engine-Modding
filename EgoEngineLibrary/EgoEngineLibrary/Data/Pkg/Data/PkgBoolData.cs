using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgBoolData : PkgDataList<Boolean>
    {
        public override string Type
        {
            get
            {
                return "bool";
            }
        }

        protected override uint DataByteSize
        {
            get
            {
                return 1;
            }
        }

        public override int Align
        {
            get
            {
                return 4;
            }
        }

        public PkgBoolData(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            UInt32 numData = ReadHeader(reader);

            for (int i = 0; i < numData; ++i)
            {
                values.Add(reader.ReadBoolean());
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (bool val in values)
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
            bool res = Boolean.Parse(data);
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
