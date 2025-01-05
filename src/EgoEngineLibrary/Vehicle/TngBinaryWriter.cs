﻿using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class TngBinaryWriter : EndianBinaryWriter
    {
        public TngBinaryWriter(LittleEndianBitConverter bitConverter, System.IO.Stream stream)
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
        public void WriteTerminatedString(string s, byte terminator)
        {
            this.Write(Encoding.UTF8.GetBytes(s));
            this.Write(terminator);
        }
    }
}
