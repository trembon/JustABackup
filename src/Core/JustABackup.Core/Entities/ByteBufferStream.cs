using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JustABackup.Core.Entities
{
    public class ByteBufferStream : Stream
    {
        protected long length;
        protected long position;

        protected List<byte> internalBuffer;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => length;

        public override long Position { get => position; set => throw new NotImplementedException(); }

        public ByteBufferStream()
        {
            internalBuffer = new List<byte>();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readSize = count;
            if (readSize > internalBuffer.Count)
                readSize = internalBuffer.Count;

            internalBuffer.CopyTo(0, buffer, offset, readSize);
            internalBuffer.RemoveRange(0, readSize);

            position += readSize;

            return readSize;
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            int internalbufferSize = 2 * 1024 * 1024;

            int readBytes = 0;
            byte[] buffer = new byte[internalbufferSize];
            while ((readBytes = await this.ReadAsync(buffer, 0, internalbufferSize)) > 0)
                await destination.WriteAsync(buffer, 0, readBytes, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            internalBuffer.AddRange(buffer.Skip(offset).Take(count));
            length += count;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            internalBuffer?.Clear();
            internalBuffer = null;

            length = -1;
            position = -1;
        }
    }
}
