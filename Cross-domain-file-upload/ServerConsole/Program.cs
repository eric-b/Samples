using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Owin.Hosting;
using NLog;

namespace ServerConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Logger logger = null;
            var disposables = new List<IDisposable>();
            try
            {
                logger = LogManager.GetLogger("ServerConsole");
                var container = App_Start.SimpleInjectorInitializer.SetUp();
                var demoContext = (DemoContext)container.GetService(typeof(DemoContext));

                foreach (var port in demoContext.GetPorts())
                {
                    var localhostBaseAddress = string.Format("http://localhost:{0}/", port);

                    var options = new StartOptions();
                    options.Urls.Add(localhostBaseAddress);

                    disposables.Add(WebApp.Start<App_Start.OwinAppStartup>(options));

                    // Warm-up
                    using (var client = new HttpClient())
                    {
                        var notUsed = client.GetAsync(string.Format(localhostBaseAddress, "UploadPage")).Result;
                    }
                }

                logger.Info("\r\nBrowse http://localhost:{0}/UploadPage\r\n...",
                    demoContext.Server1Port);
                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Fatal(ex);
                else
                    Console.WriteLine(ex.ToString());
                Console.ReadKey(true);
            }
            finally
            {
                try
                {
                    foreach (var d in disposables)
                        d.Dispose();
                }
                catch
                {
                }
            }
        }
    }
}