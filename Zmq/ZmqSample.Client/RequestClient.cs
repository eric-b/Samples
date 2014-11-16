using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NetMQ;
using ZmqSample.Client.Extensions;
using ZmqSample.Client.Model;

namespace ZmqSample.Client
{
    public sealed class RequestClient : IRequestClient
    {
        private readonly NetMQSocket _socket;
        private readonly NetMQContext _context;
        private int _disposeCount;

        public RequestClient(string address)
        {
            _context = NetMQContext.Create();
            _socket = _context.CreateRequestSocket();
            _socket.Connect(address);
            Console.WriteLine("RequestClient: connected to {0}", address);
        }

        public ResponseMsg Send(RequestMsg msg)
        {
            if (_disposeCount != 0)
                throw new ObjectDisposedException(this.GetType().Name);

            try
            {
                _socket.Send(msg.SerializeMessage());
                return _socket.Receive().DeserializeMessage() as ResponseMsg;
            }
            catch (TerminatingException)
            {
                Debug.WriteLine(string.Format("TerminatingException: auto-disposing {0}...", this.GetType().Name));
                ((IDisposable)this).Dispose();
                throw;
            }
        }

        void IDisposable.Dispose()
        {
            if (Interlocked.Increment(ref _disposeCount) != 1)
                return;
            Debug.WriteLine(string.Format("Disposing {0}...", this.GetType().Name));
            new Action(() => _socket.Options.Linger = TimeSpan.Zero).CaptureMqExceptions<TerminatingException>();
            new Action(() => _socket.Dispose()).CaptureMqExceptions();
            new Action(() => _context.Dispose()).CaptureMqExceptions();
            Console.WriteLine(string.Format("{0} disposed.", this.GetType().Name));
        }
    }
}