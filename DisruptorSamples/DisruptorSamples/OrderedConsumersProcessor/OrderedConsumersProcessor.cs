using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using DisruptorSamples.MultipleConsumersProcessor;

namespace DisruptorSamples.OrderedConsumersProcessor
{
    public sealed class OrderedConsumersProcessor : IDisposable
    {
        private readonly Disruptor<Event> _disruptor;
        private int _disposeCount;
        private RingBuffer<Event> _ringBuffer;
        private bool _started;

        public OrderedConsumersProcessor(OrderedConsumersProcessorOptions options)
        {
            _disruptor = new Disruptor<Event>(() => new Event(), options.BufferLength, TaskScheduler.Default);

            var logHandler = new LogEventHandler();
            var zipHandler = new ZipEventHandler();
            _disruptor.HandleEventsWith(logHandler).Then(zipHandler);
        }

        public void Start()
        {
            _ringBuffer = _disruptor.Start();
            _started = true;
        }

        public bool Publish(string filepath)
        {
            long seqNo;
            try
            {
                seqNo = _ringBuffer.Next(TimeSpan.FromMilliseconds(500));
            }
            catch(TimeoutException)
            {
                return false;
            }
            try
            {
                Event entry = _ringBuffer[seqNo];
                entry.Filepath = filepath;
            }
            finally
            {
                _ringBuffer.Publish(seqNo);
            }
            return true;
        }

        void IDisposable.Dispose()
        {
            if (Interlocked.Increment(ref _disposeCount) != 1)
                return;
            _disruptor.Shutdown();
        }
    }
}