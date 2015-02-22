using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Identity2.Sample1.WebHost.Infrastructure.Identity;
using Identity2.Sample1.WebHost.Infrastructure.Ldap;
using Microsoft.AspNet.Identity;
using SimpleInjector;
using SimpleInjector.Integration.Web.Mvc;

namespace Identity2.Sample1.WebHost
{
    public static class SimpleInjectorConfig
    {
        public static void Configure()
        {
            var container = new Container();
            
            ConfigureServices(container);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.RegisterMvcIntegratedFilterProvider();
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }

        private static void ConfigureServices(Container container)
        {
            container.RegisterSingle<ILdapAuthClient, FakeLdapAuthClient>();

            container.RegisterSingle<FakeUserStore>();
            container.RegisterSingle<IUserRoleStore<UserIdentity, int>>(() => container.GetInstance<FakeUserStore>());
            container.RegisterSingle<UserManager<UserIdentity, int>, ApplicationUserManager>();
        }
    }
}