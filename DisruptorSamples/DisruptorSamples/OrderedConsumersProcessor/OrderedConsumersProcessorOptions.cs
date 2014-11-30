using System;
using System.Linq;

namespace DisruptorSamples.OrderedConsumersProcessor
{
    public sealed class OrderedConsumersProcessorOptions
    {
        private int _bufferLength;

        public OrderedConsumersProcessorOptions()
        {
            BufferLength = 4;
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
    }
}