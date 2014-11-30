namespace EgoEngineLibrary.Archive
{
    using MiscUtil.Conversion;
    using MiscUtil.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class JpkBinaryReader : EndianBinaryReader
    {
        public JpkBinaryReader(LittleEndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
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
    }
}
