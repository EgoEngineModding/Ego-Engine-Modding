using System.Text;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Text;

public static class EncodingExtensions
{
    extension(Encoding encoding)
    {
        public StringByteBuffer BuildStringByteBuffer(IEnumerable<string> strings)
        {
            return encoding.BuildStringByteBuffer([.. strings]);
        }

        public StringByteBuffer BuildStringByteBuffer(IReadOnlyCollection<string> strings)
        {
            int totalCharacters = 0;
            int largestStringLength = 0;
            foreach (var s in strings)
            {
                totalCharacters += s.Length;
                largestStringLength = Math.Max(largestStringLength, s.Length);
            }

            int bufferSize = encoding.GetMaxByteCount(totalCharacters);
            int singleBufferSize = encoding.GetMaxByteCount(largestStringLength);
            int bufferIndex = 0;
            byte[] buffer = new byte[bufferSize + strings.Count];
            byte[] singleBuffer = new byte[singleBufferSize + 1];
            var stringOffsetMap = new Dictionary<string, int>(strings.Count);
            foreach (var s in strings)
            {
                if (stringOffsetMap.ContainsKey(s))
                {
                    continue;
                }
            
                var bytesWritten = encoding.GetBytes(s, singleBuffer);
                singleBuffer[bytesWritten] = 0;
                var filenameSpan = singleBuffer.AsSpan(0, bytesWritten + 1);
                var index = buffer.IndexOf(filenameSpan);
                if (index >= 0)
                {
                    stringOffsetMap.Add(s, index);
                }
                else
                {
                    stringOffsetMap.Add(s, bufferIndex);
                    filenameSpan.CopyTo(buffer.AsSpan(bufferIndex));
                    bufferIndex += filenameSpan.Length;
                }
            }
        
            return new StringByteBuffer(buffer, bufferIndex, stringOffsetMap); 
        }
    }
}