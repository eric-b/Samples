using System;
using System.Linq;
using ZmqSample.Client.Model;

namespace ZmqSample.Client
{
    public interface IRequestClient : IDisposable
    {
        ResponseMsg Send(RequestMsg msg);
    }
}