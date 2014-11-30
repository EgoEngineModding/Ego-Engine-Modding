using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EgoEngineLibrary.Helper
{
    public static class HexHelper
    {
        private static readonly uint[] _lookup32 = CreateLookup32();
        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("x2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }
        public static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 3];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[3 * i] = (char)val;
                result[3 * i + 1] = (char)(val >> 16);
                result[3 * i + 2] = ' ';
            }
            return new string(result);
        }
        public static byte[] HexToByteUsingByteManipulation(string s)
        {
            byte[] bytes = new byte[s.Length / 3];
            for (int i = 0; i < bytes.Length; i++)
            {
                int hi = s[i * 3] - 65;
                hi = hi + 10 + ((hi >> 31) & 7);

                int lo = s[i * 3 + 1] - 65;
                lo = lo + 10 + ((lo >> 31) & 7) & 0x0f;

                bytes[i] = (byte)(lo | hi << 4);
            }
            return bytes;
        }
        public static string ByteArrayToHexViaLookup32(byte[] bytes, int elementsPerRow)
        {
            uint[] lookup32 = _lookup32;
            char[] result = new char[bytes.Length * 3 + 1 + ((bytes.Length - 1) / elementsPerRow)];
            int j = -1, elems = 0;
            result[++j] = '\n';
            for (int i = 0; i < bytes.Length; i++, elems++)
            {
                if (elementsPerRow - elems == 0)
                {
                    result[++j] = '\n';
                    elems = 0;
                }
                uint val = lookup32[bytes[i]];
                char valc = (char)val;
                if (valc != '0')
                {
                    result[++j] = valc;
                }
                result[++j] = (char)(val >> 16);
                result[++j] = ' ';
            }
            return new string(result).TrimEnd('\0');
        }
    }
}
