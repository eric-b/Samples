using System;
using System.Linq;
using NetMQ;
using ZmqReqRep.Common;

namespace ZmqReqRep.Server
{
    public sealed class RepServer : IDisposable
    {
        private readonly NetMQContext _ctx;
        private readonly RepSocket[] _serverSocket;
        private readonly RouterDealerQueueDevice _dealerDevice;

        public RepServer(string address, int nodeCount)
        {
            _ctx = NetMQContext.Create();

            if (nodeCount == 1)
            {
                _serverSocket = new RepSocket[]
                {
                    new RepSocket(address, ConnectionMode.Bind, _ctx)
                };
            }
            else
            {
                string backendAddress = "inproc://repserver/cluster";
                _dealerDevice = new RouterDealerQueueDevice(address, backendAddress, ConnectionMode.Bind, _ctx);

                _serverSocket = new RepSocket[nodeCount];
                for (int i = 0; i < nodeCount; i++)
                    _serverSocket[i] = new RepSocket(backendAddress, ConnectionMode.Connect, _ctx);
            }
        }
        
        public void Dispose()
        {
            try
            {
                foreach (var item in _serverSocket)
                    item.Dispose();
            }
            finally
            {
                _ctx.Dispose();
            }
            if (_dealerDevice == null && _serverSocket.Length > 1)
                throw new ApplicationException("Impossible exception: only used to trick compilation optimization with unused variable _dealerDevice.");
        }
    }
}