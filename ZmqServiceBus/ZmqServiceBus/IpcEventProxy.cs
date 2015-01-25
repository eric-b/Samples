using System;
using System.Linq;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using ZmqServiceBus.Exceptions;
using ZmqServiceBus.Internals;
using ZmqServiceBus.Sockets;

namespace ZmqServiceBus
{ 
    /// <summary>
    /// <para>IPC Event Proxy for inter-process communications.</para>
    /// <para>Only one instance of this class must be created for all process.</para>
    /// </summary>
    public sealed class IpcEventProxy : IDisposable
    {
        private readonly NetMQContext _context;
        private readonly ITraceWriter _traces;
        private readonly Thread _bgThread;
        private readonly string _xSubscriberAddress, _xPublisherAddress;

        private readonly bool _verboseLog;

        private int _disposeCount;
        private bool _isFaultedState;

        /// <summary>
        /// This proxy is not operational any more.
        /// </summary>
        public event EventHandler<EventArgs> FaultedState;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="publisherAddress">XPublisher address (for example: tcp://*:9002).</param>
        /// <param name="subscriberAddress">XPublisher address (for example: tcp://*:9001).</param>
        public IpcEventProxy(string publisherAddress, string subscriberAddress) : this(publisherAddress, subscriberAddress, false, EmptyTraceWriter.Instance)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="publisherAddress">XPublisher address (for example: tcp://*:9002).</param>
        /// <param name="subscriberAddress">XPublisher address (for example: tcp://*:9001).</param>
        /// <param name="traceWriter">Traces</param>
        /// <param name="verboseLog"></param>
        public IpcEventProxy(string publisherAddress, string subscriberAddress, bool verboseLog, ITraceWriter traceWriter)
        {
            if (string.IsNullOrEmpty(publisherAddress))
                throw new ArgumentNullException("publisherAddress");
            if (string.IsNullOrEmpty(subscriberAddress))
                throw new ArgumentNullException("subscriberAddress");
            if (traceWriter == null)
                throw new ArgumentNullException("traceWriter");

            _traces = traceWriter;
            _context = NetMQContext.Create();

            _verboseLog = verboseLog;
            _xSubscriberAddress = subscriberAddress;
            _xPublisherAddress = publisherAddress;

            _bgThread = new Thread(new ThreadStart(ProxyThread));
            _bgThread.IsBackground = true;
            _bgThread.Name = "IpcEventProxy";
            _bgThread.Start();

            if (!TrySync(TimeSpan.FromSeconds(1)))
            {
                Dispose();
                throw new SyncException();
            }
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCount) != 1)
                return;

            try
            {
                _context.Dispose();
                _traces.Debug("IpcEventProxy disposed.");
            }
            catch (NetMQException ex)
            {
                _traces.Error(ex);
            }
        }

        /// <summary>
        /// <para>Blocks until the proxy is operational.</para>
        /// </summary>
        /// <returns>true en cas de succès.</returns>
        private bool TrySync(TimeSpan timeout)
        {
            string pubAddress = _xSubscriberAddress.StartsWith("tcp://*") ? _xSubscriberAddress.Replace("*", "localhost") : _xSubscriberAddress;
            string subAddress = _xPublisherAddress.StartsWith("tcp://*") ? _xPublisherAddress.Replace("*", "localhost") : _xPublisherAddress;
            var publisher = new Publisher(pubAddress, _context, _verboseLog, _traces);
            try
            {
                var synchronizer = new SocketSynchronizer(_context, pubAddress, subAddress, timeout, _verboseLog, _traces);
                if (synchronizer.TrySync(publisher))
                    return !_isFaultedState;
            }
            finally
            {
                publisher.Terminate();
            }
            return false;
        }

        private void ProxyThread()
        {
            if (_disposeCount != 0)
            {
                _traces.Debug("IpcEventProxy: disposed before start.");
                return;
            }
            XSubscriberSocket xsub = null;
            XPublisherSocket xpub = null;
            try
            {
                xsub = _context.CreateXSubscriberSocket();
                xpub = _context.CreateXPublisherSocket();

                xsub.Bind(_xSubscriberAddress);
                xpub.Bind(_xPublisherAddress);
                var xproxy = new Proxy(xpub, xsub, null);
                _traces.Debug("IpcEventProxy: started (pub->xsub {0} <=> {1} xpub<-sub)", _xSubscriberAddress, _xPublisherAddress);
                xproxy.Start();
                _traces.Debug("IpcEventProxy: stopped.");
            }
            catch (NetMQException ex)
            {
                if (_disposeCount == 0 && !(ex is TerminatingException))
                {
                    _isFaultedState = true;
                    _traces.Error(ex, "Error while IpcEventProxy starting or during operation.");
                    var handler = FaultedState;
                    if (handler != null)
                    {
                        try
                        {
                            handler(this, EventArgs.Empty);
                        }
                        catch (Exception ex2)
                        {
                            _traces.Error(ex2);
                        }
                    }
                }
            }
            finally
            {
                if (xsub != null)
                    _traces.CaptureMqExceptions(xsub.Dispose);
                if (xpub != null)
                    _traces.CaptureMqExceptions(xpub.Dispose);
            }
        }
    }
}