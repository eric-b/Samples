using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

internal sealed class HashedStream : Stream
{
    private readonly Stream _baseStream;
    private readonly HashAlgorithm _hashAlgo;
    private readonly bool _leaveOpen;
    private bool _writeMode, _readMode, _finalized, _disposed;

    /// <summary>
    /// Gets the calculated hash (read this property at the end of the stream).
    /// </summary>
    public byte[] Hash
    {
        get
        {
            return _hashAlgo.Hash;
        }
    }

    public override long Length
    {
        get
        {
            return _baseStream.Length;
        }
    }

    public override bool CanRead
    {
        get
        {
            return _baseStream.CanRead;
        }
    }

    public override long Position
    {
        get
        {
            return _baseStream.Position;
        }
        set
        {
            _baseStream.Position = value;
        }
    }

    public override bool CanSeek
    {
        get
        {
            return _baseStream.CanSeek;
        }
    }

    public override bool CanWrite
    {
        get
        {
            return _baseStream.CanWrite;
        }
    }

    public HashedStream(Stream stream, HashAlgorithm hashAlgo) : this(stream, hashAlgo, doNotDisposeResources: false)
    {

    }

    public HashedStream(Stream stream, HashAlgorithm hashAlgo, bool doNotDisposeResources)
    {
        if (stream == null)
            throw new ArgumentNullException("stream");
        if (hashAlgo == null)
            throw new ArgumentNullException("hashAlgo");
        _baseStream = stream;
        _hashAlgo = hashAlgo;
        _leaveOpen = doNotDisposeResources;
    }

    /// <summary>
    /// If the underlying stream is written, call this method before reading Hash property.
    /// </summary>
    public override void Flush()
    {
        _baseStream.Flush();
        if (!_finalized && !_readMode)
        {
            _hashAlgo.TransformFinalBlock(new byte[0], 0, 0);
            _finalized = true;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _baseStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _baseStream.SetLength(value);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        const int emptyCount = 0;
        _readMode |= true;
        int bytesRead = _baseStream.Read(buffer, offset, count);
        if (bytesRead == emptyCount)
        {
            if (!_finalized)
            {
                _hashAlgo.TransformFinalBlock(buffer, 0, 0);
                _finalized = true;
            }
            return emptyCount;
        }
        _hashAlgo.TransformBlock(buffer, 0, bytesRead, buffer, 0);
        return bytesRead;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_readMode)
            throw new InvalidOperationException("Cannot write after read operation.");
        if (_finalized)
            throw new InvalidOperationException("Cannot write after flush operation. You can use autoflush if wanted.");
        _writeMode |= true;
        _baseStream.Write(buffer, offset, count);
        _hashAlgo.TransformBlock(buffer, offset, count, buffer, offset);
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;
        base.Dispose(disposing);
        if (!_readMode)
            Flush();
        if (disposing && !_leaveOpen)
        {
            _baseStream.Dispose();
            _hashAlgo.Dispose();
        }
    }
}