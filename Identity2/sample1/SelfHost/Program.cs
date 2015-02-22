using System;
using System.Linq;
using Identity2.Sample1.SelfHost.Infrastructure.Owin;
using Microsoft.Owin.Hosting;

namespace Identity2.Sample1.SelfHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                const string baseAddress = "http://localhost:9000/externalApi/";
                using (WebApp.Start<OwinStartup>(baseAddress))
                {
                    Console.WriteLine("Host started: {0}", baseAddress);
                    Console.ReadKey(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }
    }
}
