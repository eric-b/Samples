using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NetMQ;
using ZmqServiceBus.Sockets;

namespace ZmqServiceBus.Internals
{
    internal sealed class SocketSynchronizer
    {
        private static readonly long[] SyncEventSingleFilter = new long[] { SyncEvent.EventTopic };

        private readonly string _subAddress, _pubAddress;
        private readonly NetMQContext _context;
        private readonly bool _verboseLog;
        private readonly ITraceWriter _traces;
        private readonly TimeSpan _syncTimeout;

        public SocketSynchronizer(NetMQContext context, string publisherAddress, string subscriberAddress, TimeSpan syncTimeout, bool verboseLog, ITraceWriter traceWriter)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (string.IsNullOrEmpty(publisherAddress))
                throw new ArgumentNullException("publisherAddress");
            if (string.IsNullOrEmpty(subscriberAddress))
                throw new ArgumentNullException("subscriberAddress");
            if (syncTimeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("syncTimeout");
            if (traceWriter == null)
                throw new ArgumentNullException("traceWriter");

            _traces = traceWriter;
            _verboseLog = verboseLog;
            _pubAddress = publisherAddress;
            _subAddress = subscriberAddress;
            _context = context;
            _syncTimeout = syncTimeout;
        }

        public bool TrySync(Publisher pub)
        {
            Subscriber syncSub = new Subscriber(_subAddress, _context, SyncEventSingleFilter, _syncTimeout, _verboseLog, _verboseLog ? _traces : EmptyTraceWriter.Instance);
            _traces.Debug("Try sync pub {0:x} with special subscriber {1:x}...", pub.InstanceId, syncSub.InstanceId);
            try
            {
                return TrySync(pub, syncSub, syncSub.InstanceId);
            }
            finally
            {
                _traces.Debug("Terminating syncsub {0:x}.", syncSub.InstanceId);
                syncSub.Terminate();
            }
        }

        public bool TrySync(Subscriber sub)
        {
            var syncPub = new Publisher(_pubAddress, _context, _verboseLog, _verboseLog ? _traces : EmptyTraceWriter.Instance);
            _traces.Debug("Try sync sub {0:x} with special publisher {1:x}...", sub.InstanceId, syncPub.InstanceId);
            try
            {
                return TrySync(syncPub, sub, sub.InstanceId);
            }
            finally
            {
                _traces.Debug("Terminating syncpub {0:x}.", syncPub.InstanceId);
                syncPub.Terminate();
            }
        }

        private bool TrySync(Publisher publisher, Subscriber subscriber, int subcriberInstanceId)
        {
            var syncMsg = new SyncEvent(subcriberInstanceId);
            SpinWait spin = new SpinWait();
            Stopwatch watch = Stopwatch.StartNew();
            bool retry = true;
            while (retry)
            {
                publisher.Publish<SyncEvent>(SyncEvent.EventTopic, syncMsg);
                for (int i = 0; i < 2; i++)
                    spin.SpinOnce();

                if (subscriber.IsSynced)
                {
                    if (_verboseLog)
                        _traces.Debug("Socket sync performed in {0:F0} ms (spin count: {1}).", watch.ElapsedMilliseconds, spin.Count);
                    return true;
                }
                else if (watch.Elapsed > _syncTimeout)
                    return false;
            }
            return false;
        }
    }
}