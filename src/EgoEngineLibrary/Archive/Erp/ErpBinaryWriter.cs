using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Archive.Erp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ErpBinaryWriter : EndianBinaryWriter
    {
        public ErpBinaryWriter(EndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
        }

        public new void Write(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            this.Write(data);
            this.Write((byte)0x0);
        }
        public void Write(string str, int length)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            this.Write(data);

            length = length - data.Length;
            if (length > 0)
            {
                this.Write(new byte[length]);
            }
        }
    }
}
