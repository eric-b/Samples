using System;
using System.Linq;
using System.Web.Http.Dependencies;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace ServerConsole.App_Start
{
    internal static class SimpleInjectorInitializer
    {
        private static readonly object DependencyResolverSync = new object();

        private static IDependencyResolver _dependencyResolver;

        public static IDependencyResolver SetUp()
        {
            if (_dependencyResolver != null)
                return _dependencyResolver;
            lock (DependencyResolverSync)
            {
                if (_dependencyResolver != null)
                    return _dependencyResolver;

                var container = new Container();

                Configure(container);

                _dependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
                return _dependencyResolver;
            }
        }

        private static void Configure(Container container)
        {
            container.RegisterSingle<DemoContext>(() =>
            {
                var ports = Helper.GetAvailablePort(2);
                return new DemoContext(ports[0], ports[1]);
            });
        }
    }
}