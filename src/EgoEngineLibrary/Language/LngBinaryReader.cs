using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class LngBinaryReader : EndianBinaryReader
    {
        public LngBinaryReader(BigEndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
        }

        public string ReadTerminatedString(byte terminator)
        {
            string filename = "";
            List<byte> fnBytes = new List<byte>();
            byte filenameByte = this.ReadByte();
            while (filenameByte != terminator)
            {
                filename += (char)filenameByte;
                fnBytes.Add(filenameByte);
                filenameByte = this.ReadByte();
            }
            return Encoding.UTF8.GetString(fnBytes.ToArray());
        }
    }
}
