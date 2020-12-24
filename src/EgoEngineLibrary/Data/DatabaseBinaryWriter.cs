namespace EgoEngineLibrary.Data
{
    using MiscUtil.Conversion;
    using MiscUtil.IO;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class DatabaseBinaryWriter : EndianBinaryWriter
    {
        public DatabaseBinaryWriter(EndianBitConverter bitConverter, Stream stream)
            : base(bitConverter, stream)
        {
        }

        public void WriteDatabaseString(string s, int maxLength)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            base.Write(bytes);
            int num = maxLength % 4;
            int num2 = ((num == 3) ? (maxLength + 5) : (maxLength + (4 - num))) - bytes.Length;
            while (num2-- > 0)
            {
                base.Write((byte)0);
            }
        }

        public void WriteTerminatedString(string s, byte terminator)
        {
            base.Write(Encoding.UTF8.GetBytes(s));
            base.Write(terminator);
        }
    }
}
