using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureBlobStoreTester
{
    public class CountdownLatch
    {
        private int _remain;
        private EventWaitHandle _event;

        public CountdownLatch(int count)
        {
            _remain = count;
            _event = new ManualResetEvent(false);
        }

        public void Signal()
        {
            if (Interlocked.Decrement(ref _remain) == 0)
            {
                _event.Set();
            }
        }

        public void Wait()
        {
            _event.WaitOne();
        }
    }
}
