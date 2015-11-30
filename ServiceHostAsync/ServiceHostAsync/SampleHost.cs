using System;
using System.Threading;

namespace ServiceHostAsync
{
    sealed class SampleHost : IDisposable
    {
        private readonly ILogger _logger;

        public SampleHost(ILogger logger)
        {
            _logger = logger;
            Thread.Sleep(5000);
            //throw new Exception("test");
        }

        public void Dispose()
        {
            Thread.Sleep(21000);
            _logger.Debug("Host disposed.");
        }
    }
}
