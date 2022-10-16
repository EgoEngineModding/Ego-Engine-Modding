using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace EgoEngineLibrary.IO;

public sealed class ConcatenatedStream : Stream
{
    private readonly Queue<Stream> _streams;
    private readonly bool _leaveOpen;
    private bool _isOpen;

    public ConcatenatedStream(IEnumerable<Stream> streams, bool leaveOpen = true)
    {
        _streams = new Queue<Stream>(streams);
        _leaveOpen = leaveOpen;
        _isOpen = true;
    }

    public override bool CanRead => _isOpen;

    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowIfClosed();
        var bytesRead = 0;
        while (_streams.Count > 0)
        {
            bytesRead = _streams.Peek().Read(buffer, offset, count);
            if (bytesRead != 0 || count == 0)
            {
                break;
            }
            
            var stream = _streams.Dequeue();
            if (_leaveOpen)
            {
                stream.Dispose();
            }
        }

        return bytesRead;
    }

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override void Flush()
    {
    }

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
    
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (!disposing)
            {
                return;
            }

            if (!_leaveOpen)
            {
                while (_streams.Count > 0)
                {
                    _streams.Dequeue().Dispose();
                }
            }
            _isOpen = false;
        }
        finally
        {
            // Call base.Close() to cleanup async IO resources
            base.Dispose(disposing);
        }
    }
    
    private void ThrowIfClosed()
    {
        if (!_isOpen)
        {
            ThrowClosedException();
        }
    }

    [DoesNotReturn]
    private static void ThrowClosedException()
    {
        throw new ObjectDisposedException(nameof(ConcatenatedStream), "Stream is closed.");
    }
}