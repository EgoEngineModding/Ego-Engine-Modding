namespace EgoEngineLibrary.Data
{
    using MiscUtil.Conversion;
    using MiscUtil.IO;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class DatabaseBinaryReader : EndianBinaryReader
    {
        public DatabaseBinaryReader(LittleEndianBitConverter bitConverter, Stream stream)
            : base(bitConverter, stream)
        {
        }

        public string ReadDatabaseString(int maxLength)
        {
            int num = maxLength % 4;
            int count = (num == 3) ? (maxLength + 5) : (maxLength + (4 - num));
            return Encoding.UTF8.GetString(base.ReadBytes(count)).TrimEnd(new char[1]);
        }

        public string ReadTerminatedString(byte terminator)
        {
            List<byte> list = new List<byte>();
            for (byte i = base.ReadByte(); i != terminator; i = base.ReadByte())
            {
                list.Add(i);
            }
            return Encoding.UTF8.GetString(list.ToArray());
        }
    }
}
