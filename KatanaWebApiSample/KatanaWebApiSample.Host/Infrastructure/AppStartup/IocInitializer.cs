using System;
using System.Web.Http.Dependencies;
using KatanaWebApiSample.Host.Domain;
using KatanaWebApiSample.WebApiLib;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace KatanaWebApiSample.Host.Infrastructure.AppStartup
{
    internal static class IocInitializer
    {
        /// <summary>
        /// <para>Set up the IoC container.</para>
        /// </summary>
        /// <returns></returns>
        public static IDependencyResolver SetUp()
        {
            var container = new Container();
            Configure(container);
            return new SimpleInjectorWebApiDependencyResolver(container);
        }

        /// <summary>
        /// Set up the services in the IoC container.
        /// </summary>
        /// <param name="container"></param>
        private static void Configure(Container container)
        {
            container.RegisterSingle<IMyService, MyService>();
        }
    }
}