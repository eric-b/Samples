using System;
using System.Linq;
using Disruptor;

namespace DisruptorSamples.MultipleConsumersProcessor
{
    internal sealed class LogEventHandler : IEventHandler<Event>
    {
        void IEventHandler<Event>.OnNext(Event data, long sequence, bool endOfBatch)
        {
            Console.WriteLine("{1:HH:mm:ss.fff}: File path processing: {0}.", data.Filepath, DateTime.Now);
        }
    }
}