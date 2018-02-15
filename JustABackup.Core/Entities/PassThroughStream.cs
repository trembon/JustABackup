using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace JustABackup.Core.Entities
{
    public class PassThroughStream : Stream
    {
        private long length = 0;
        private long position = 0;

        private object readWriteLock = new object();
        private List<byte> buffer = new List<byte>();
        private bool isComplete = false;

        private AutoResetEvent wait = new AutoResetEvent(false);

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => length;

        public override long Position { get => position; set => position = value; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readSize = 0;

            do
            {
                lock (readWriteLock)
                {
                    if (this.buffer.Count > 0)
                    {
                        var bytes = this.buffer.Take(count).ToList();

                        readSize = bytes.Count;
                        position += readSize;

                        bytes.CopyTo(buffer, offset);

                        this.buffer = this.buffer.Skip(count).ToList();
                    }
                }

                if (readSize == 0 && !isComplete)
                    wait.WaitOne(100);

            } while (readSize == 0 && !isComplete);

            return readSize;

            //bool hasBuffer = false;
            //lock (readWriteLock)
            //{
            //    hasBuffer = this.buffer.Count > 0;
            //}

            //if (!hasBuffer && !isComplete)
            //    wait.WaitOne();

            //lock (readWriteLock)
            //{
            //    if (this.buffer.Count > 0)
            //    {
            //        var bytes = this.buffer.Take(count).ToList();
            //        readSize = bytes.Count;
            //        bytes.CopyTo(buffer, offset);

            //        this.buffer = this.buffer.Skip(count).ToList();
            //    }
            //}

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
            lock (readWriteLock)
            {
                this.buffer.AddRange(buffer.Skip(offset).Take(count));
                length += count;
            }
            wait.Set();
        }

        public void SetComplete()
        {
            isComplete = true;
            wait.Set();
        }
    }
}
