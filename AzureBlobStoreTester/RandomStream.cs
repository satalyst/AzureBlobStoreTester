using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobStoreTester
{
    /// <summary>
    /// A <see cref="Stream"/>  implementation that returns random data up to the number of bytes specified.
    /// </summary>
    public class RandomStream : Stream
    {
        private readonly Random _random;

        private readonly long _size = 0;
        private long _bytesWritten = 0;

        public RandomStream(long size)
        {
            this._size = size;
            this._random = new Random();
        }

        public long Remaining
        {
            get { return _size - _bytesWritten; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count > Remaining)
            {
                // can safely make this cast, we know that the int value is greater than the long, so it will definitely
                // fit inside an int.
                count = (int)Remaining;
            }

            byte[] data = new byte[count];
            _random.NextBytes(data);

            Buffer.BlockCopy(data, 0, buffer, offset, count);

            _bytesWritten += count;
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get { return _bytesWritten < _size; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return _size; }
        }

        public override long Position
        {
            get { return _bytesWritten; } 
            set { throw new NotImplementedException();}
        }
    }
}
