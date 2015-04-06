using System;
using System.Linq;

namespace ZmqReqRep.Client
{
    public interface IReqSocket
    {
        string SendRequest(string request);
    }
}