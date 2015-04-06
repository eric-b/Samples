using System;
using System.Diagnostics;
using System.Linq;

namespace ZmqReqRep.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var address = "tcp://localhost:1040";
                using (var ctx = NetMQ.NetMQContext.Create())
                    using (var client = new ReqSocket(address, ctx))
                    {
                        Console.WriteLine("Client connected.");
                        var watch = Stopwatch.StartNew();
                        int i = 0;
                        while (watch.Elapsed < TimeSpan.FromSeconds(2))
                        {
                            string response = client.SendRequest(string.Format("Request #{0}", ++i));
                            Console.WriteLine(response);
                        }
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("\r\nPress a key to exit...");
                Console.ReadKey(true);
            }
        }
    }
}