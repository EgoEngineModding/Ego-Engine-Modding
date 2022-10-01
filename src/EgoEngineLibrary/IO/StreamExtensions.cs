using System;
using System.IO;

namespace EgoEngineLibrary.IO;

public static class StreamExtensions
{
    public static void ReadExactly(this Stream stream, Span<byte> buffer)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        var totalRead = 0;
        var count = buffer.Length;
        while (totalRead < count)
        {
            var readCount = stream.Read(buffer[totalRead..]);
            if (readCount == 0)
                throw new EndOfStreamException("The end of the stream is reached.");
            totalRead += readCount;
        }
    }
}