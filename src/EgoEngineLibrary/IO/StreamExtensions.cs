using System;
using System.IO;

namespace EgoEngineLibrary.IO;

public static class StreamExtensions
{
    public static void ReadExactly(this Stream stream, Span<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var totalRead = 0;
        var count = buffer.Length;
        while (totalRead < count)
        {
            var readCount = stream.Read(buffer[totalRead..]);
            if (readCount == 0)
            {
                throw new EndOfStreamException("The end of the stream is reached.");
            }

            totalRead += readCount;
        }
    }
    
    public static bool TryReadExactly(this Stream stream, Span<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var totalRead = 0;
        var count = buffer.Length;
        while (totalRead < count)
        {
            var readCount = stream.Read(buffer[totalRead..]);
            if (readCount == 0)
            {
                return false;
            }

            totalRead += readCount;
        }

        return true;
    }
}