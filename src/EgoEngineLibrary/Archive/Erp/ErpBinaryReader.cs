using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Archive.Erp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ErpBinaryReader : EndianBinaryReader
    {
        public ErpBinaryReader(System.IO.Stream stream)
            : base(EndianBitConverter.Little, stream)
        {

        }
        public ErpBinaryReader(EndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {

        }

        public new string ReadString()
        {
            List<byte> fnBytes = new List<byte>();
            byte filenameByte = this.ReadByte();

            while (filenameByte != 0x00)
            {
                fnBytes.Add(filenameByte);
                filenameByte = this.ReadByte();
            }

            return Encoding.UTF8.GetString(fnBytes.ToArray());
        }

        public string ReadString(int length)
        {
            byte[] bytes = this.ReadBytes(length);

            int startEnd = bytes.Length;
            for (int i = 0; i < bytes.Length; ++i)
            {
                if (bytes[i] == 0x00)
                {
                    startEnd = i;
                    break;
                }
            }

            return Encoding.UTF8.GetString(bytes, 0, startEnd);
        }
    }
}
