using System.Runtime.InteropServices;

namespace EgoEngineLibrary.IO;

public static class EndianBinaryReaderExtensions
{
    extension(EndianBinaryReader reader)
    {
        public string ReadNullTerminatedString()
        {
            List<byte> bytes = [];
            byte b = reader.ReadByte();

            while (b != 0x00)
            {
                bytes.Add(b);
                b = reader.ReadByte();
            }

            return reader.Encoding.GetString(CollectionsMarshal.AsSpan(bytes));
        }
    }
}