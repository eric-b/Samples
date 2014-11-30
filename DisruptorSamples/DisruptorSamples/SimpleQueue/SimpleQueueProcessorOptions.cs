using System;
using System.Linq;

namespace DisruptorSamples.SimpleQueue
{
    public sealed class SimpleQueueProcessorOptions
    {
        private int _bufferLength;

        public SimpleQueueProcessorOptions()
        {
            BufferLength = 4;
            EnableZip = true;
        }

        public int BufferLength
        {
            get
            {
                return _bufferLength;
            }
            set
            {
                _bufferLength = value.NextPowerOfTwo();
            }
        }

        public bool EnableZip { get; set; }
    }
}