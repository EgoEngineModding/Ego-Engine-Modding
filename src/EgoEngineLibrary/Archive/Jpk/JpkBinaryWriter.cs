using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Archive.Jpk
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class JpkBinaryWriter : EndianBinaryWriter
    {
        public JpkBinaryWriter(LittleEndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
        }

        public void WriteObject(object data)
        {
            if (data.GetType() == typeof(int))
            {
                Write((int)data);
            }
            else if (data.GetType() == typeof(float))
            {
                Write((float)data);
            }
            else
            {
                throw new Exception("The writer does not recognize this data type! (" + data.GetType() + ")");
            }
        }
        public new void Write(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            this.Write(data);
            this.Write((byte)0x0);
        }
        public void WriteTerminatedString(string s, byte terminator)
        {
            this.Write(Encoding.UTF8.GetBytes(s));
            this.Write(terminator);
        }
    }
}
