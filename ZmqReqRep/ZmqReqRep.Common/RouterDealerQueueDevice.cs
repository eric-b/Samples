using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace ZmqReqRep.Common
{
    public sealed class RouterDealerQueueDevice
    {
        private readonly NetMQContext _ctx;
        private readonly string _frontendAddress, _backendAddress;
        private readonly ConnectionMode _backendCxMode;
        private readonly Thread _bgThread;

        public RouterDealerQueueDevice(string frontEndAddress, string backEndAddress, ConnectionMode backendCxMode, NetMQContext zmqContext)
        {
            _ctx = zmqContext;
            _backendCxMode = backendCxMode;
            _frontendAddress = frontEndAddress;
            _backendAddress = backEndAddress;
           
            _bgThread = new Thread(new ThreadStart(ProxyThread));
            _bgThread.IsBackground = true;
            _bgThread.Start();

            #region If device is client side...
            if (_frontendAddress.StartsWith("inproc://") &&
                    !TrySyncInProcSocket(_frontendAddress, 1000))
            {
                throw new TimeoutException();
            } 
            #endregion

            #region If device is server side...
            if (backendCxMode == ConnectionMode.Bind && _backendAddress.StartsWith("inproc://")
                && !TrySyncInProcSocket(_backendAddress, 1000))
            {
                throw new TimeoutException();
            }
            #endregion
        }

        private void ProxyThread()
        {
            RouterSocket router = null;
            DealerSocket dealer = null;
            try
            {
                router = _ctx.CreateRouterSocket();
                dealer = _ctx.CreateDealerSocket();
                router.Bind(_frontendAddress);
                switch (_backendCxMode)
                {
                    case ConnectionMode.Connect:
                        dealer.Connect(_backendAddress);
                        break;
                    case ConnectionMode.Bind:
                        dealer.Bind(_backendAddress);
                        break;
                }

                router.Options.Linger = TimeSpan.Zero;
                dealer.Options.Linger = TimeSpan.Zero;
                var xproxy = new Proxy(router, dealer, null);
                xproxy.Start();
            }
            catch (TerminatingException)
            {
            }
            finally
            {
                if (router != null)
                {
                    try
                    {
                        router.Dispose();
                    }
                    catch (NetMQException)
                    {
                    }
                }
                if (dealer != null)
                {
                    try
                    {
                        dealer.Dispose();
                    }
                    catch (NetMQException)
                    {
                    }
                }
            }
        }
        
        private bool TrySyncInProcSocket(string connectAddress, int timeout)
        {
            if (string.IsNullOrEmpty(connectAddress))
                throw new ArgumentNullException("connectAddress");
            if (timeout < 0)
                throw new ArgumentOutOfRangeException("timeout", "La valeur ne peut être négative.");
            using (DealerSocket socket = _ctx.CreateDealerSocket())
            {
                SpinWait spin = new SpinWait();
                var watch = Stopwatch.StartNew();
                while (true)
                {
                    try
                    {
                        socket.Connect(connectAddress);
                        return true;
                    }
                    catch (TerminatingException)
                    {
                        return false;
                    }
                    catch (EndpointNotFoundException) // netMQ > 3.3.0.11
                    {
                        if (watch.ElapsedMilliseconds > timeout)
                            return false;

                        spin.SpinOnce();
                    }
                    catch (InvalidException) // netMQ = 3.3.0.11
                    {
                        if (watch.ElapsedMilliseconds > timeout)
                            return false;

                        spin.SpinOnce();
                    }
                }
            }
        }
    }
}