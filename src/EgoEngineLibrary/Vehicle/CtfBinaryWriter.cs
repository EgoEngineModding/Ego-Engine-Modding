using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CtfBinaryWriter : EndianBinaryWriter
    {
        public CtfBinaryWriter(LittleEndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
        }

        public void WriteEntryData(string type, object data)
        {
            switch (type)
            {
                case "int":
                    Write((int)data);
                    break;
                case "float":
                    Write((float)data);
                    break;
                case "double":
                    Write((double)data);
                    break;
                case "bool":
                    Write(Convert.ToInt32((bool)data));
                    break;
                case "string":
                    WriteTerminatedString((string)data, new byte());
                    break;
                case "float-list":
                    WriteFloatList((FloatList)data);
                    break;
                default:
                    throw new Exception("An entry in the ctfSchema file has an incorrect type!");
            }
        }
        public void WriteTerminatedString(string s, byte terminator)
        {
            this.Write(Encoding.UTF8.GetBytes(s));
            this.Write(terminator);
        }
        public void WriteFloatList(FloatList fList)
        {
            Write(fList.count);
            Write(fList.step);
            for (int i = 0; i < fList.items.Length; i++)
            {
                Write(fList.items[i]);
            }
        }
    }
}
