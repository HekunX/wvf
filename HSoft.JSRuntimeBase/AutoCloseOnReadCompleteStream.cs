﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSoft.JSRuntimeBase
{
    public class AutoCloseOnReadCompleteStream : Stream
    {
        private readonly Stream _baseStream;

        public AutoCloseOnReadCompleteStream(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }

        public override void Flush() => _baseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = _baseStream.Read(buffer, offset, count);

            // Stream.Read only returns 0 when it has reached the end of stream
            // and no further bytes are expected. Otherwise it blocks until
            // one or more (and at most count) bytes can be read.
            if (bytesRead == 0)
            {
                _baseStream.Close();
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

        public override void SetLength(long value) => _baseStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);
    }
}
