using System;
using Microsoft.Owin.Hosting;

namespace KatanaWebApiSample.Host
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            const string baseAddress = "http://localhost:9000/";
            using (WebApp.Start<Infrastructure.AppStartup.OwinAppStartup>(baseAddress))
            {
                var client = new System.Net.Http.HttpClient();
                var requestUri = string.Format("{0}api/SayHello/Smith", baseAddress);
                Console.WriteLine("GET {0} ...", requestUri);
                var response = client.GetAsync(requestUri).Result;
                Console.WriteLine("{0}\r\n{1}", response, response.Content.ReadAsStringAsync().Result);
                Console.ReadKey(true);
            }
        }
    }
}