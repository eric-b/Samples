using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;

namespace DisruptorSamples.SimpleQueue
{
    public sealed class SimpleQueueProcessor : IEventHandler<Event>, IDisposable
    {
        private readonly Disruptor<Event> _disruptor;
        private int _disposeCount;
        private RingBuffer<Event> _ringBuffer;
        private bool _started;
        private readonly bool _enableZip;

        public SimpleQueueProcessor(SimpleQueueProcessorOptions options)
        {
            _enableZip = options.EnableZip;
            _disruptor = new Disruptor<Event>(() => new Event(), options.BufferLength, TaskScheduler.Default);
            _disruptor.HandleEventsWith(this);
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

        void IEventHandler<Event>.OnNext(Event data, long sequence, bool endOfBatch)
        {
            if (!_enableZip)
                return;
            try
            {
                var zipPath = string.Format("{0}.zip", data.Filepath);
                using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(
                        data.Filepath,
                        Path.GetFileName(data.Filepath),
                        CompressionLevel.Optimal);
                }
                Console.WriteLine(string.Format("ZIP created: {0}", zipPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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