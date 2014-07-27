using System;
using System.Configuration;
using System.Linq;
using System.Web.Http.Dependencies;
using RsaRijndaelWebApi.Infrastructure.Cryptography;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace RsaRijndaelWebApi.Infrastructure.AppStartup
{
    public class IocInitializer
    {
        private static IDependencyResolver _dependencyResolver;
        private static readonly object DependencyResolverSync = new object();

        public static IDependencyResolver SetUp()
        {
            if (_dependencyResolver != null)
                return _dependencyResolver;

            lock (DependencyResolverSync)
            {
                if (_dependencyResolver != null)
                    return _dependencyResolver;
                var container = new Container(new ContainerOptions()
                {
                    AllowOverridingRegistrations = false
                });

                Configure(container);

                _dependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
                return _dependencyResolver;
            }
        }

        private static void Configure(Container container)
        {
            container.RegisterSingle<Cipher>(() => new Cipher(ConfigurationManager.AppSettings["privateKey"]));
        }
    }
}