namespace EgoEngineLibrary.Archive.Jpk
{
    using MiscUtil.Conversion;
    using MiscUtil.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class JpkBinaryReader : EndianBinaryReader
    {
        public JpkBinaryReader(EndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
        }

        public new string ReadString()
        {
            List<byte> fnBytes = new List<byte>();
            byte filenameByte;

            while ((filenameByte = this.ReadByte()) != 0x00)
            {
                fnBytes.Add(filenameByte);
            }

            return Encoding.UTF8.GetString(fnBytes.ToArray());
        }
    }
}
