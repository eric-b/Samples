using System;
using SimpleInjector;
using ServiceHostAsync.Infrastructure;

namespace ServiceHostAsync.AppStart
{
    internal static class SimpleInjectorInitializer
    {
        public static IServiceProvider SetUp()
        {
            var container = new Container();
            Configure(container);
            return container;
        }
        
        private static void Configure(Container container)
        {
            container.RegisterSingleton<ILogger>(() => new NLogLogger(NLog.LogManager.GetLogger("Log")));

            container.Register(() => new Func<SampleHost>(() => container.GetInstance<SampleHost>()));
        }
    }
}