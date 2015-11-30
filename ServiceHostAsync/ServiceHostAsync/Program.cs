using ServiceHostAsync.Infrastructure;
using System;

namespace ServiceHostAsync
{
    static class Program
    {
        private static ILogger _logger;

        private static void Main(string[] args)
        {
            _logger = new NLogLogger(NLog.LogManager.GetLogger("Log"));
            int exitCode = 0;
            try
            {
                if (!Environment.UserInteractive)
                    ServiceHost<SampleHost>.RunAsService(new Func<IServiceProvider>(() => AppStart.SimpleInjectorInitializer.SetUp()), _logger);
                else
                    ServiceHost<SampleHost>.RunInConsole(new Func<IServiceProvider>(() => AppStart.SimpleInjectorInitializer.SetUp()), _logger, args);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                exitCode = -1;
            }
            Environment.Exit(exitCode);
        }
    }
}
