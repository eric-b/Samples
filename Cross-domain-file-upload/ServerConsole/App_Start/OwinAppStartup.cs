using System;
using System.Linq;
using System.Web.Http;
using Owin;
using Microsoft.Owin;

namespace ServerConsole.App_Start
{
    internal class OwinAppStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {   
            HttpConfiguration webApiConfig = new HttpConfiguration();
            webApiConfig.DependencyResolver = SimpleInjectorInitializer.SetUp();
            WebApiConfig.Register(webApiConfig);
            appBuilder.UseWebApi(webApiConfig);
        }
    }
}