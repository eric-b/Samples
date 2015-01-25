using System;
using System.Linq;
using System.Threading;
using NetMQ;
using ZmqServiceBus.Exceptions;
using ZmqServiceBus.Internals;
using ZmqServiceBus.Sockets;

namespace ZmqServiceBus
{
    /// <summary>
    /// <para>Implementation of <see cref="IServiceBus"/> for inter-process communication.</para>
    /// <para>Note that while methods of IpcServiceBus are thread-safe,
    /// instances of IPublisher and ISubscriber managed by this class are not thread-safe.</para>
    /// </summary>
    public sealed class IpcServiceBus : IServiceBus
    {
        private readonly SocketSynchronizer _synchronizer;
        
        private readonly NetMQContext _context;

        private readonly ITraceWriter _traces;

        private readonly bool _verboseLog;

        private readonly string _publisherAddress, _subscriberAddress;

        private int _socketCount;

        private int _disposeCount;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="publisherAddress"></param>
        /// <param name="subscriberAddress"></param>
        public IpcServiceBus(string publisherAddress, string subscriberAddress) : this(publisherAddress, subscriberAddress, false, EmptyTraceWriter.Instance)
        {

        }

        public IpcServiceBus(string publisherAddress, string subscriberAddress, bool verboseLog, ITraceWriter traceWriter)
        {
            if (traceWriter == null)
                throw new ArgumentNullException("traceWriter");
            _traces = traceWriter;
            _verboseLog = verboseLog;
            _publisherAddress = publisherAddress;
            _subscriberAddress = subscriberAddress;
            _context = NetMQContext.Create();
            _synchronizer = new SocketSynchronizer(_context, _publisherAddress, _subscriberAddress, TimeSpan.FromMilliseconds(500), _verboseLog, _traces);
        }

        public ISubscriber CreateSubscriber(params long[] subscribeToEventCodes)
        {
            if (_disposeCount != 0)
                throw new ObjectDisposedException(this.GetType().Name);
            if (subscribeToEventCodes == null)
                throw new ArgumentNullException("subscribeToEventCodes");

            Interlocked.Increment(ref _socketCount);
            Subscriber subscriber = null;
            try
            {
                if (subscribeToEventCodes.Length != 0)
                {
                    long[] subscribeToEventCodesExtended = new long[subscribeToEventCodes.Length + 1];
                    Array.Copy(subscribeToEventCodes, subscribeToEventCodesExtended, subscribeToEventCodes.Length);
                    subscribeToEventCodesExtended[subscribeToEventCodesExtended.Length - 1] = SyncEvent.EventTopic;
                    subscribeToEventCodes = subscribeToEventCodesExtended;
                }
                subscriber = new Subscriber(_subscriberAddress, _context, subscribeToEventCodes, TimeSpan.MaxValue, _verboseLog, _traces);
                if (!_synchronizer.TrySync(subscriber))
                    throw new SyncException();
                return subscriber;
            }
            catch (Exception ex)
            {
                _traces.Error(ex);
                if (subscriber != null)
                    Release(subscriber);
                else
                    Interlocked.Decrement(ref _socketCount);
                throw;
            }
        }

        public IPublisher CreatePublisher()
        {
            if (_disposeCount != 0)
                throw new ObjectDisposedException(this.GetType().Name);

            Interlocked.Increment(ref _socketCount);

            try
            {
                var publisher = new Publisher(_publisherAddress, _context, _verboseLog, _traces);
                if (!_synchronizer.TrySync(publisher))
                    throw new SyncException();
                return publisher;
            }
            catch (Exception ex)
            {
                Interlocked.Decrement(ref _socketCount);
                _traces.Error(ex);
                throw;
            }
        }

        public void Release(ISubscriber instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            var socket = instance as Subscriber;
            if (socket == null)
                throw new ArgumentException("The specified instance does not match the type of Publisher managed by this class.", "instance");

            if (socket.Terminate())
                Interlocked.Decrement(ref _socketCount);
        }

        public void Release(IPublisher instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            var socket = instance as Publisher;
            if (socket == null)
                throw new ArgumentException("The specified instance does not match the type of Publisher managed by this class.", "instance");

            if (socket.Terminate())
                Interlocked.Decrement(ref _socketCount);
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCount) != 1)
                return;
            _traces.CaptureMqExceptions(_context.Dispose);
        }
    }
}