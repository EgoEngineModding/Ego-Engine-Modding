using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgShnmData : PkgDataList<String>
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
            UInt32 numData = ReadHeader(reader);

            for (int i = 0; i < numData; ++i)
            {
                values.Add(reader.ReadString(16));
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (string val in values)
            {
                writer.Write(val, 16);
            }
        }

        public override string GetData(Int32 index)
        {
            return Type + " " + values[index];
        }
        public override Int32 SetData(string data)
        {
            int index = values.IndexOf(data);

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
