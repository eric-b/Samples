using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ZmqReqRep.Tests
{
    internal static class Helper
    {
        public static int GetAvailablePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }
    }
}