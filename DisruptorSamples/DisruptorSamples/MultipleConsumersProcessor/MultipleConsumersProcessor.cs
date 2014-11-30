using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;

namespace DisruptorSamples.MultipleConsumersProcessor
{
    public sealed class MultipleConsumersProcessor : IDisposable
    {
        private readonly Disruptor<Event> _disruptor;
        private int _disposeCount;
        private RingBuffer<Event> _ringBuffer;
        private bool _started;

        public MultipleConsumersProcessor(MultipleConsumersProcessorOptions options)
        {
            _disruptor = new Disruptor<Event>(() => new Event(), options.BufferLength, TaskScheduler.Default);

            var logHandler = new LogEventHandler();
            var zipHandler = new ZipEventHandler();
            _disruptor.HandleEventsWith(logHandler, zipHandler);
        }

        public void Start()
        {
            _ringBuffer = _disruptor.Start();
            _started = true;
        }

        public void Publish(string filepath)
        {
            if (_disposeCount != 0)
                throw new ObjectDisposedException(this.GetType().Name);
            if (!_started)
                throw new InvalidOperationException("Method Start() must be called before this method.");
            if (string.IsNullOrEmpty(filepath))
                throw new ArgumentNullException("filepath");
            long seqNo;
            seqNo = _ringBuffer.Next();
            try
            {
                Event entry = _ringBuffer[seqNo];
                entry.Filepath = filepath;
            }
            finally
            {
                _ringBuffer.Publish(seqNo);
            }
        }

        void IDisposable.Dispose()
        {
            if (Interlocked.Increment(ref _disposeCount) != 1)
                return;
            _disruptor.Shutdown();
        }
    }
}