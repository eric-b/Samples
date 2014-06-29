using System;
using KatanaWebApiSample.WebApiLib;

namespace KatanaWebApiSample.Host.Domain
{
    /// <summary>
    /// Implementation of IMyService. 
    /// If this service is application domain specific, 
    /// this class should typically be in another assembly 
    /// (avoid to mix application host project with app domain projects).
    /// </summary>
    internal class MyService : IMyService
    {
        public string SayHello(string name)
        {
            return string.Format("Hello {0}", name);
        }
    }
}