using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using ZmqReqRep.Common;

namespace ZmqReqRep.Server
{
    public sealed class RepSocket : IDisposable
    {
        private readonly Task _bgTask;
        private readonly CancellationTokenSource _bgTaskCts;
        private readonly NetMQContext _ctx;
        private readonly string _address;
        private readonly ConnectionMode _cxMode;
        
        public RepSocket(string address, ConnectionMode cxMode, NetMQContext context)
        {
            _ctx = context;
            _address = address;
            _cxMode = cxMode;
            _bgTaskCts = new CancellationTokenSource();
            _bgTask = new Task(BackgroundTask, _bgTaskCts.Token, _bgTaskCts.Token, TaskCreationOptions.LongRunning);
            _bgTask.Start();
        }
        
        private void BackgroundTask(object state)
        {
            var cancellationToken = (CancellationToken)state;
            ResponseSocket socket = _ctx.CreateResponseSocket();
            try
            {
                switch (_cxMode)
                {
                    case ConnectionMode.Connect:
                        socket.Connect(_address);
                        break;
                    case ConnectionMode.Bind:
                        socket.Bind(_address);
                        break;
                }
                
                byte[] receiveBuffer;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        receiveBuffer = socket.Receive();
                        if (receiveBuffer == null)
                            continue; // NetMQ > 3.3.0.11
                    }
                    catch (AgainException)
                    {
                        continue; // NetMQ = 3.3.0.11
                    }

                    #region Always send a reply...
                    
                    try
                    {
                        Thread.Sleep(500); // simulates processing...
                        socket.Send(string.Format("Reply ({0})", Encoding.UTF8.GetString(receiveBuffer)));
                    }
                    catch (TerminatingException)
                    {
                        try
                        {
                            socket.Send("Exit...");
                        }
                        catch
                        {
                        }
                        throw;
                    }
                    
                    #endregion
                }
            }
            catch (TerminatingException)
            {
            }
            finally
            {
                try
                {
                    socket.Dispose();
                }
                catch (NetMQException)
                {
                }
            }
        }
        
        public void Dispose()
        {
            _bgTaskCts.Cancel();
        }
    }
}