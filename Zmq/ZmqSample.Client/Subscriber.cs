using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using ZmqSample.Client.Extensions;

namespace ZmqSample.Client
{
    public sealed class Subscriber : IDisposable
    {
        private readonly NetMQContext _context;
        private readonly CancellationTokenSource _backgroundSocketTaskCts;
        private readonly Task _backgroundSocketTask;
        private readonly string _connectAddress, _filter;
        private int _disposeCount;

        public Subscriber(string address, string filter)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("address");
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("filter");
            _context = NetMQContext.Create();
            _backgroundSocketTaskCts = new CancellationTokenSource();
            _backgroundSocketTask = new Task(ListenSocket, _backgroundSocketTaskCts.Token, _backgroundSocketTaskCts.Token, TaskCreationOptions.LongRunning);
            _connectAddress = address;
            _filter = filter;
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

            Debug.WriteLine(string.Format("Disposing {0} {1}...", this.GetType().Name, _filter));

            _backgroundSocketTaskCts.Cancel();
            new Action(() => _context.Dispose()).CaptureMqExceptions();
            Console.WriteLine(string.Format("{0} {1} disposed.", this.GetType().Name, _filter));
        }

        private void ListenSocket(object state)
        {
            var cancellationToken = (CancellationToken)state;
            try
            {
                using (var socket = _context.CreateSubscriberSocket())
                {
                    socket.Subscribe(_filter);
                    socket.Connect(_connectAddress);
                    Console.WriteLine("Subscriber {1}: connected to {0}", _connectAddress, _filter);
                    byte[] buffer;
                    while (_disposeCount == 0 && !cancellationToken.IsCancellationRequested)
                    {
                        buffer = new Func<byte[]>(() => socket.Receive()).CaptureMqExceptions<AgainException, byte[]>();
                        if (buffer == null)
                            continue;
                        // filter received, listen for message following:
                        buffer = new Func<byte[]>(() => socket.Receive()).CaptureMqExceptions<AgainException, byte[]>();

                        var message = buffer.DeserializeMessage();
                        Console.WriteLine("{2}: msg recd on {1:HH:mm:ss.fff}: {0}", message, DateTime.UtcNow, _filter);
                        // TODO : process message [...] 
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