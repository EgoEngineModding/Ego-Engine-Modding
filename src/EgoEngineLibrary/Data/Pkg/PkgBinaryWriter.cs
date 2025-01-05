using System;
using System.Text;

using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgBinaryWriter : EndianBinaryWriter
    {
        public PkgBinaryWriter(System.IO.Stream stream)
            : base(EndianBitConverter.Little, stream, Encoding.UTF8, false)
        {
        }
        public PkgBinaryWriter(EndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream, Encoding.UTF8, false)
        {
        }

        public void Write(PkgOffsetType offsetType)
        {
            var temp = (uint)offsetType.Type << 24;
            temp += (uint)offsetType.Offset & 0x00FFFFFF;
            Write(temp);
        }

        public new void Write(string str)
        {
            var data = Encoding.GetBytes(str);
            Write(Convert.ToByte(data.Length));
            Write(data);
            Write((byte)0x0);
        }
        public void Write(string str, int length)
        {
            var data = Encoding.GetBytes(str);
            Write(data);

            length -= data.Length;
            if (length > 0)
            {
                Write(new byte[length]);
            }
        }
    }
}
