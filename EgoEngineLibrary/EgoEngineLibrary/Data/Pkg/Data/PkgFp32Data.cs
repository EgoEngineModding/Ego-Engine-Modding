using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public class PkgFp32Data : PkgDataList<Single>
    {
        public override string Type
        {
            get
            {
                return "fp32";
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

        public PkgFp32Data(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            UInt32 numData = ReadHeader(reader);

            for (int i = 0; i < numData; ++i)
            {
                values.Add(reader.ReadSingle());
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            WriteHeader(writer);

            foreach (float f in values)
            {
                writer.Write(f);
            }
        }

        public override string GetData(Int32 index)
        {
            return Type + " " + values[index].ToString("0.##################", CultureInfo.InvariantCulture);
        }
        public override Int32 SetData(string data)
        {
            float res = Single.Parse(data, CultureInfo.InvariantCulture);
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
