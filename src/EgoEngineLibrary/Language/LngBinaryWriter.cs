using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class LngBinaryWriter : EndianBinaryWriter
    {
        public LngBinaryWriter(BigEndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
        }

        public void WriteTerminatedString(string s, byte terminator)
        {
            this.Write(Encoding.UTF8.GetBytes(s));
            this.Write(terminator);
        }
    }
}
