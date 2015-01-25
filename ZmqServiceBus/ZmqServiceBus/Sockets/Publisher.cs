using System;
using System.IO;
using System.Linq;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using NetMQ.zmq;
using ProtoBuf;
using ZmqServiceBus.Exceptions;

namespace ZmqServiceBus.Sockets
{
    internal sealed class Publisher : IPublisher
    {
        private readonly ITraceWriter _traces;
        private readonly PublisherSocket _socket;
        private readonly bool _verboseLog;
        private readonly int _instanceHashCode;
        private int _terminateCount;
        
        internal int InstanceId
        {
            get
            {
                return _instanceHashCode;
            }
        }

        /// <summary>
        /// <para>Constructs a publisher socket.</para>
        /// </summary>
        /// <param name="address">Address to which to connect (for example: tcp://localhost:9001).</param>
        /// <param name="context">ZMQ Context.</param>
        /// <param name="verboseLog">Enable tracing of each event published.</param>
        /// <param name="traceWriter">Traces</param>
        internal Publisher(string address, NetMQContext context, bool verboseLog, ITraceWriter traceWriter)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("address");
            if (traceWriter == null)
                throw new ArgumentNullException("traceWriter");

            _traces = traceWriter;
            _verboseLog = verboseLog;
            _instanceHashCode = this.GetHashCode();

            _socket = context.CreatePublisherSocket();
            _socket.Options.SendHighWatermark = 100000;
            try
            {
                _socket.Connect(address);
                _traces.Debug("Publisher({0:x}) created ({1}).", _instanceHashCode, address);
            }
            catch (Exception ex)
            {
                _traces.Error(ex);
                _socket.Dispose();
                throw;
            }
        }

        public void Publish<T>(long eventCode, T message) where T : class
        {
            if (message == null)
                throw new ArgumentNullException("message");
            const int int64Length = 8;
            var ms = new MemoryStream();
            try
            {
                try
                {
                    Serializer.Serialize<T>(ms, message);
                }
                catch (InvalidOperationException ex)
                {
                    throw new SerializationException(ex, "Error when serializing event of type {0}. Check if that type is a valid Protobuf contract (annotated with ProtoContractAttribute).", typeof(T).FullName);
                }
                catch (Exception ex)
                {
                    throw new SerializationException(ex, "Error when serializing event of type {0}.", typeof(T).FullName);
                }
                byte[] buffer = ms.ToArray();
                _socket.Send(BitConverter.GetBytes(eventCode), int64Length, SendReceiveOptions.SendMore);
                _socket.Send(buffer, buffer.Length, SendReceiveOptions.None);
                if (_verboseLog)
                    _traces.Debug("Publisher({0:x}): event {1} sent.", _instanceHashCode, eventCode);
            }
            catch (TerminatingException)
            {
                // We ignore errors if ZMQ context is in termination phase.
            }
            finally
            {
                ms.Dispose();
            }
        }

        internal bool Terminate()
        {
            if (Interlocked.Increment(ref _terminateCount) != 1)
                return false;

            _traces.CaptureMqExceptions(() => _socket.Options.Linger = TimeSpan.Zero);
            if (_traces.CaptureMqExceptions(_socket.Dispose))
                _traces.Debug("Publisher({0:x}) disposed.", _instanceHashCode);
            return true;
        }
    }
}