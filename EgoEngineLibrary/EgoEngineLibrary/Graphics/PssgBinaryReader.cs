namespace EgoEngineLibrary.Graphics
{
    using MiscUtil.Conversion;
    using MiscUtil.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class PssgBinaryReader : EndianBinaryReader
    {
        public PssgBinaryReader(EndianBitConverter bitConvertor, System.IO.Stream stream)
            : base(bitConvertor, stream)
        {
        }

        public string ReadPSSGString()
        {
            int length = this.ReadInt32();
            return Encoding.UTF8.GetString(this.ReadBytes(length));
        }
        public string ReadPSSGString(int length)
        {
            return Encoding.UTF8.GetString(this.ReadBytes(length));
        }

        public object ReadAttributeValue(Type valueType, int size)
        {
            if (valueType == typeof(string))
            {
                return this.ReadPSSGString();
            }
            else if (valueType == typeof(UInt16))
            {
                return this.ReadUInt16();
            }
            else if (valueType == typeof(UInt32))
            {
                return this.ReadUInt32();
            }
            else if (valueType == typeof(Int16))
            {
                return this.ReadInt16();
            }
            else if (valueType == typeof(Int32))
            {
                return this.ReadInt32();
            }
            else if (valueType == typeof(Single))
            {
                return this.ReadSingle();
            }
            else if (valueType == typeof(bool))
            {
                return this.ReadBoolean();
            }
            else if (valueType == typeof(byte[]))
            {
                return this.ReadBytes(size);
            }
            else if (valueType == typeof(Single[]))
            {
                int length = size / 4;
                Single[] ret = new Single[length];
                for (int i = 0; i < length; i++)
                {
                    ret[i] = this.ReadSingle();
                }
                return ret;
            }
            else if (valueType == typeof(UInt16[]))
            {
                int length = size / 2;
                UInt16[] ret = new UInt16[length];
                for (int i = 0; i < length; i++)
                {
                    ret[i] = this.ReadUInt16();
                }
                return ret;
            }
            else // Null, or Unsupported Type
            {
                if (size > 4)
                {
                    int strlen = this.ReadInt32();
                    if (size - 4 == strlen)
                    {
                        return this.ReadPSSGString(strlen);
                    }
                    else
                    {
                        this.Seek(-4, System.IO.SeekOrigin.Current);
                    }
                }

                object data = this.ReadBytes(size);
                // Guess that data is not supposed to be byte array
                if (((byte[])data).Length == 4)
                {
                    UInt32 temp = EndianBitConverter.Big.ToUInt32((byte[])data, 0);
                    if (temp > 1000000000)
                    {
                        data = EndianBitConverter.Big.ToSingle((byte[])data, 0);
                    }
                    else
                    {
                        data = temp;
                    }
                }
                else if (((byte[])data).Length == 2)
                {
                    data = EndianBitConverter.Big.ToUInt16((byte[])data, 0);
                }

                return data;
            }
        }
        public object ReadNodeValue(Type valueType, int size)
        {
            if (valueType == typeof(Single[]))
            {
                int length = size / 4;
                Single[] ret = new Single[length];
                for (int i = 0; i < length; i++)
                {
                    ret[i] = this.ReadSingle();
                }
                return ret;
            }
            else if (valueType == typeof(UInt16[]))
            {
                int length = size / 2;
                UInt16[] ret = new UInt16[length];
                for (int i = 0; i < length; i++)
                {
                    ret[i] = this.ReadUInt16();
                }
                return ret;
            }
            else // Null, or Unsupported Type
            {
                return this.ReadBytes(size);
            }
        }
    }
}
