using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using ProtoBuf;

namespace ZmqServiceBus.Sockets
{
    internal sealed class Subscriber : ISubscriber
    {
        private readonly ITraceWriter _traces;
        private readonly NetMQContext _context;
        private readonly string _address;
        private readonly byte[][] _topicsToSubscribe;
        private readonly bool _verboseLog;
        private readonly int _instanceHashCode;
        private readonly CancellationTokenSource _backgroundSocketTaskCts;
        private readonly Task _backgroundSocketTask;
        private volatile bool _synced;
        private int _terminateCount;
        private readonly TimeSpan _recTimeout;

        public event EventHandler<MessageEventArgs> OnMessage;

        /// <summary>
        /// Gets true if a sync event targetting 
        /// this instance has been received.
        /// </summary>
        internal bool IsSynced
        {
            get
            {
                return _synced;
            }
        }

        /// <summary>
        /// Unique id for this instance
        /// (used for synchronization mechanism).
        /// </summary>
        internal int InstanceId
        {
            get
            {
                return _instanceHashCode;
            }
        }

        /// <summary>
        /// <para>Constructeur: crée un socket Subscriber.</para>
        /// <para>S'il s'agit d'un socket in process, le serveur doit d'abord
        /// être en écoute. S'il s'agit d'un socket TCP, cette restriction ne s'applique pas.</para>
        /// </summary>
        /// <param name="address">Address to which to connect (for example: tcp://localhost:9002).</param>
        /// <param name="context">ZMQ Context.</param>
        /// <param name="eventCodesToSubscribe">Identifies the event types to which to subscribe.</param>
        /// <param name="verboseLog">Enabled traces for each event received.</param>
        /// <param name="traceWriter">Traces</param>
        internal Subscriber(string address, NetMQContext context, long[] eventCodesToSubscribe, TimeSpan recTimeout, bool verboseLog, ITraceWriter traceWriter)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("address");
            if (traceWriter == null)
                throw new ArgumentNullException("traceWriter");
            if (eventCodesToSubscribe == null)
                throw new ArgumentNullException("eventCodesToSubscribe");
            
            _topicsToSubscribe = eventCodesToSubscribe.Select(t => BitConverter.GetBytes(t)).ToArray();
            _traces = traceWriter;
            _verboseLog = verboseLog;
            _context = context;
            _address = address;
            _recTimeout = recTimeout;
            _instanceHashCode = this.GetHashCode();
            _backgroundSocketTaskCts = new CancellationTokenSource();
            _backgroundSocketTask = new Task(ListenSocket, _backgroundSocketTaskCts.Token, _backgroundSocketTaskCts.Token, TaskCreationOptions.LongRunning);
            _backgroundSocketTask.Start();
        }

        internal bool Terminate()
        {
            if (Interlocked.Increment(ref _terminateCount) != 1)
                return false;
            _backgroundSocketTaskCts.Cancel();
            return true;
        }

        private void ListenSocket(object state)
        {
            var cancellationToken = (CancellationToken)state;
            var socket = _context.CreateSubscriberSocket();
            try
            {
                socket.Options.ReceiveTimeout = _recTimeout;
                socket.Options.ReceiveHighWatermark = 100000;
                socket.Connect(_address);

                if (_topicsToSubscribe.Length != 0)
                {
                    foreach (var filter in _topicsToSubscribe)
                        socket.Subscribe(filter);
                }
                else
                    socket.Subscribe(string.Empty);

                _traces.Debug("Subscriber({0:x}) created ({1}).", _instanceHashCode, _address);
                byte[] buffer;
                while (_terminateCount == 0 && !cancellationToken.IsCancellationRequested)
                {
                    buffer = _traces.CaptureMqExceptions<AgainException, byte[]>(socket.Receive);
                    if (buffer == null)
                        continue;

                    byte[] eventTopic = buffer.ToArray();
                    buffer = _traces.CaptureMqExceptions<AgainException, byte[]>(socket.Receive);
                    if (buffer == null)
                        continue;

                    if (Internals.SyncEvent.EventPrefix.SequenceEqual(eventTopic))
                    {
                        if (_synced)
                            continue;
                        using (var ms = new MemoryStream(buffer, 0, buffer.Length, false))
                        {
                            var data = Serializer.Deserialize<Internals.SyncEvent>(ms);
                            if (data.SubscriberInstanceId == _instanceHashCode)
                            {
                                if (_verboseLog)
                                    _traces.Debug("Subscriber({0:x}): sync event received.", _instanceHashCode);
                                _synced = true;
                            }
                        }
                    }
                    else
                    {
                        long eventCode = BitConverter.ToInt64(eventTopic, 0);
                        if (_verboseLog)
                            _traces.Debug("Subscriber({0:x}): event {1} received.", _instanceHashCode, eventCode);

                        var eventHandler = OnMessage;
                        if (eventHandler != null)
                        {
                            try
                            {
                                eventHandler(this, new MessageEventArgs(eventCode, buffer));
                            }
                            catch (Exception ex)
                            {
                                _traces.Error(ex, "Subscriber({0:x}): Error on receiving event {0}.", _instanceHashCode, eventCode);
                            }
                        }
                    }
                }
            }
            catch (TerminatingException)
            {
                // ZMQ context is in terminating phase...
            }
            catch (Exception ex)
            {
                _traces.Error(ex);
            }
            finally
            {
                if (_traces.CaptureMqExceptions(socket.Dispose))
                    _traces.Debug("Subscriber({0:x}) disposed.", _instanceHashCode);
            }
        }
    }
}