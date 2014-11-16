using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NetMQ;
using ZmqSample.Client.Extensions;
using ZmqSample.Client.Model;

namespace ZmqSample.Client
{
    public sealed class RequestClientFactory : IDisposable
    {
        private readonly NetMQContext _context;
        private int _disposeCount;

        public RequestClientFactory()
        {
            _context = NetMQContext.Create();
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCount) != 1)
                return;
            Debug.WriteLine(string.Format("Disposing {0}...", this.GetType().Name));
            new Action(() => _context.Dispose()).CaptureMqExceptions();
            Debug.WriteLine(string.Format("{0} disposed.", this.GetType().Name));
        }

        public IRequestClient Create(string address)
        {
            if (_disposeCount != 0)
                throw new ObjectDisposedException(this.GetType().Name);
            return new RequestClientEx(_context, address);
        }

        private class RequestClientEx : IRequestClient
        {
            private readonly NetMQSocket _socket;
            private int _disposeCount;

            public RequestClientEx(NetMQContext context, string address)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                _socket = context.CreateRequestSocket();
                _socket.Connect(address);
                Console.WriteLine("RequestClientEx: connected to {0} ({1:x})", address, this.GetHashCode());
            }

            public void Dispose()
            {
                if (Interlocked.Increment(ref _disposeCount) != 1)
                    return;
                Debug.WriteLine(string.Format("Disposing {0} ({1:x})...", this.GetType().Name, this.GetHashCode()));
                new Action(() => _socket.Options.Linger = TimeSpan.Zero).CaptureMqExceptions<TerminatingException>();
                new Action(() => _socket.Dispose()).CaptureMqExceptions();
                Console.WriteLine(string.Format("{0} disposed ({1:x}).", this.GetType().Name, this.GetHashCode()));
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
                    Debug.WriteLine(string.Format("TerminatingException: auto-disposing {0} ({1:x})...", this.GetType().Name, this.GetHashCode()));
                    ((IDisposable)this).Dispose();
                    throw;
                }
            }
        }
    }
}
