using System;
using System.Linq;

namespace DisruptorSamples.MultipleConsumersProcessor
{
    public sealed class MultipleConsumersProcessorOptions
    {
        private int _bufferLength;

        public MultipleConsumersProcessorOptions()
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