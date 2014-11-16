using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using ZmqSample.Client.Extensions;
using ZmqSample.Client.Model;

namespace ZmqSample.Server
{
    public sealed class ReplyReceiver : IDisposable
    {
        private readonly NetMQContext _context;
        private readonly CancellationTokenSource _backgroundSocketTaskCts;
        private readonly Task _backgroundSocketTask;
        private readonly string _bindAddress;
        private int _disposeCount;

        public ReplyReceiver(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("address");
            _context = NetMQContext.Create();
            _backgroundSocketTaskCts = new CancellationTokenSource();
            _backgroundSocketTask = new Task(ListenSocket, _backgroundSocketTaskCts.Token, _backgroundSocketTaskCts.Token, TaskCreationOptions.LongRunning);
            _bindAddress = address;
        }

        public void Start()
        {
            if (_disposeCount != 0)
                throw new ObjectDisposedException(this.GetType().Name);
            _backgroundSocketTask.Start();
        }

        void IDisposable.Dispose()
        {
            if (Interlocked.Increment(ref _disposeCount) != 1)
                return;

            Debug.WriteLine(string.Format("Disposing {0}...", this.GetType().Name));

            _backgroundSocketTaskCts.Cancel();
            new Action(() => _context.Dispose()).CaptureMqExceptions();
            Console.WriteLine(string.Format("{0} disposed.", this.GetType().Name));
        }

        private void ListenSocket(object state)
        {
            var cancellationToken = (CancellationToken)state;
            try
            {
                using (var socket = _context.CreateResponseSocket())
                {
                    socket.Bind(_bindAddress);
                    Console.WriteLine("ReplyReceiver: bound to {0}", _bindAddress);
                    byte[] buffer;
                    while (_disposeCount == 0 && !cancellationToken.IsCancellationRequested)
                    {
                        buffer = new Func<byte[]>(() => socket.Receive()).CaptureMqExceptions<AgainException, byte[]>();
                        if (buffer == null)
                            continue;

                        var message = buffer.DeserializeMessage() as RequestMsg;
                        if (message == null)
                            continue;
                        Console.WriteLine("Request: {0}", message);
                        socket.Send(new ResponseMsg("I don't know...").SerializeMessage());
                    }
                }
            }
            catch (TerminatingException)
            {
                Debug.WriteLine(string.Format("TerminatingException: auto-disposing {0}...", this.GetType().Name));
                ((IDisposable)this).Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            Debug.WriteLine("End of reception.");
        }
    }
}