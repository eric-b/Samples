using System;
using System.Linq;

namespace ZmqReqRep.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var address = "tcp://localhost:1040";
                using (new RepServer(address, 1))
                {
                    Console.WriteLine("Server ready: {0}", address);

                    Console.WriteLine("\r\nPress a key to exit...");
                    Console.ReadKey(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}