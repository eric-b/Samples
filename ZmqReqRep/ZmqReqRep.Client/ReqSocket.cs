using System;
using System.Linq;
using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace ZmqReqRep.Client
{
    public sealed class ReqSocket : IReqSocket, IDisposable
    {
        private static readonly TimeoutException TimeoutException = new TimeoutException();

        private readonly RequestSocket _reqSocket;

        public ReqSocket(string address, NetMQContext context)
        {
            _reqSocket = context.CreateRequestSocket();
            _reqSocket.Options.ReceiveTimeout = TimeSpan.FromSeconds(10);
            _reqSocket.Options.Linger = TimeSpan.Zero;
            try
            {
                _reqSocket.Connect(address);
            }
            catch
            {
                try
                {
                    _reqSocket.Dispose();
                }
                catch
                {
                }
                throw;
            }
        }

        public void Dispose()
        {
            _reqSocket.Dispose();
        }

        public string SendRequest(string request)
        {
            _reqSocket.Send(request);
            byte[] receiveBuffer;
            try
            {
                receiveBuffer = _reqSocket.Receive();
                if (receiveBuffer == null) // NetMQ > 3.3.0.11
                    throw TimeoutException;

                return Encoding.UTF8.GetString(receiveBuffer);
            }
            catch (TerminatingException)
            {
                Dispose();
                throw;
            }
            catch (AgainException) // NetMQ = 3.3.0.11
            {
                throw TimeoutException;
            }
        }
    }
}