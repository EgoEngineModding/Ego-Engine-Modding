using System;
using System.IO;

namespace EgoEngineLibrary.IO;

public static class StreamExtensions
{
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