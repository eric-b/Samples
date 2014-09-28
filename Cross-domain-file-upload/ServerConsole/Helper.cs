using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace ServerConsole
{
    internal static class Helper
    {
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
        private static readonly string ResourceNamespacePrefix = typeof(Program).Namespace + ".Resources.";

        public static int[] GetAvailablePort(int count)
        {
            var ports = new int[count];
            for (int i = 0; i < count; i++)
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
                    ports[i] = ((IPEndPoint)socket.LocalEndPoint).Port;
                }
            }
            return ports;
        }

        public static string GetResourceAsString(string name)
        {
            using (var stream = Assembly.GetManifestResourceStream(ResourceNamespacePrefix + name))
            {
                if (stream == null)
                    return null;
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}