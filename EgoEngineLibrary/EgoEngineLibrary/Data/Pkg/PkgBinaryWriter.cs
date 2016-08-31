using MiscUtil.Conversion;
using MiscUtil.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgBinaryWriter : EndianBinaryWriter
    {
        public PkgBinaryWriter(System.IO.Stream stream)
            : base(EndianBitConverter.Little, stream, Encoding.UTF8)
        {
        }
        public PkgBinaryWriter(EndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream, Encoding.UTF8)
        {
        }

        public void Write(PkgOffsetType offsetType)
        {
            UInt32 temp = (UInt32)offsetType.Type << 24;
            temp += (UInt32)offsetType.Offset & 0x00FFFFFF;
            Write(temp);
        }

        public new void Write(string str)
        {
            base.Write(str);
            //byte[] data = Encoding.UTF8.GetBytes(str);
            //this.Write(data);
            this.Write((byte)0x0);
        }
        public void Write(string str, int length)
        {
            byte[] data = Encoding.GetBytes(str);
            this.Write(data);

            length = length - data.Length;
            if (length > 0)
            {
                this.Write(new byte[length]);
            }
        }
    }
}
