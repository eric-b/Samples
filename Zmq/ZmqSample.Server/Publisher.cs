using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using ZmqSample.Client.Extensions;
using ZmqSample.Client.Model;

namespace ZmqSample.Server
{
    public sealed class Publisher : IDisposable
    {
        private readonly PublisherSocket _socket;
        private readonly NetMQContext _context;
        private int _disposeCount;

        public Publisher(string address)
        {
            _context = NetMQContext.Create();
            _socket = _context.CreatePublisherSocket();
            _socket.Bind(address);
            Console.WriteLine("Publisher: bound to {0}", address);
        }

        public void Publish(string filter, PushMsg message)
        {
            if (_disposeCount != 0)
                throw new ObjectDisposedException(this.GetType().Name);
            try
            {
                _socket.Send(filter, false, true);
                _socket.Send(message.SerializeMessage());
            }
            catch (TerminatingException)
            {
                Debug.WriteLine(string.Format("TerminatingException: auto-disposing {0}...", this.GetType().Name));
                ((IDisposable)this).Dispose();
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