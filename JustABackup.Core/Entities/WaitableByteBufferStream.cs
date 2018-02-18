using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JustABackup.Core.Entities
{
    public class WaitableByteBufferStream : ByteBufferStream
    {
        private object readWriteLock;
        private bool isComplete;
        
        private ManualResetEvent resetEvent;

        public WaitableByteBufferStream()
        {
            readWriteLock = new object();
            isComplete = false;

            resetEvent = new ManualResetEvent(false);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (readWriteLock)
            {
                if (isComplete && this.internalBuffer.Count == 0)
                    return 0;
            }

            resetEvent.WaitOne();

            lock (readWriteLock)
            {
                int readSize = base.Read(buffer, offset, count);

                if (!isComplete && this.internalBuffer.Count == 0)
                    resetEvent.Reset();
                
                return readSize;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (readWriteLock)
            {
                base.Write(buffer, offset, count);

                resetEvent.Set();
            }
        }

        public void SetComplete()
        {
            lock (readWriteLock)
            {
                isComplete = true;
                resetEvent.Set();
            }
        }
    }
}
