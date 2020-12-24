namespace EgoEngineLibrary.Vehicle
{
    using MiscUtil.Conversion;
    using MiscUtil.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CtfBinaryReader : EndianBinaryReader
    {
        public CtfBinaryReader(LittleEndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
        }

        public object ReadEntryData(string type)
        {
            switch (type)
            {
                case "int":
                    return ReadInt32();
                case "float":
                    return ReadSingle();
                case "double":
                    return ReadDouble();
                case "bool":
                    return Convert.ToBoolean(ReadInt32());
                case "string":
                    return ReadTerminatedString(new byte());
                case "float-list":
                    return ReadFloatList();
                default:
                    throw new Exception("An entry in the ctfSchema file has an incorrect type!");
            }
        }
        public string ReadTerminatedString(byte terminator)
        {
            List<byte> strBytes = new List<byte>();
            do
            {
                strBytes.Add(ReadByte());
            } while (strBytes[strBytes.Count - 1] != terminator);
            strBytes.RemoveAt(strBytes.Count - 1);
            return Encoding.UTF8.GetString(strBytes.ToArray());
        }
        public FloatList ReadFloatList()
        {
            FloatList fList;
            fList.count = ReadInt32();
            fList.step = ReadSingle();
            fList.items = new float[fList.count];
            for (int i = 0; i < fList.items.Length; i++)
            {
                fList.items[i] = ReadSingle();
            }
            return fList;
        }
    }
}
