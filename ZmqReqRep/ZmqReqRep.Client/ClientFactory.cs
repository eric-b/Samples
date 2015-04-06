using System;
using System.Linq;
using NetMQ;
using ZmqReqRep.Common;

namespace ZmqReqRep.Client
{
    public sealed class ClientFactory : IDisposable
    {
        private readonly NetMQContext _ctx;
        private readonly RouterDealerQueueDevice _sharedQueue;
        private readonly string _frontendAddress;

        public ClientFactory(string address)
        {
            _ctx = NetMQContext.Create();
            try
            {
                _frontendAddress = "inproc://demo-zmq";
                _sharedQueue = new RouterDealerQueueDevice(_frontendAddress, address, ConnectionMode.Connect, _ctx);
            }
            catch
            {
                try
                {
                    _ctx.Dispose();
                }
                catch
                {
                }
                throw;
            }
        }

        public IReqSocket Create()
        {
            return new ReqSocket(_frontendAddress, _ctx);
        }

        public void Release(IReqSocket instance)
        {
            ((ReqSocket)instance).Dispose();
        }
        
        public void Dispose()
        {
            _ctx.Dispose();
            if (_sharedQueue == null)
                throw new ApplicationException("Impossible exception: only used to trick compilation optimization with unused variable _sharedQueue.");
        }
    }
}