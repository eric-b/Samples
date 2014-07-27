using System;
using System.Linq;
using System.Web.Http;

namespace RsaRijndaelWebApi.Infrastructure.AppStartup
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.DependencyResolver = IocInitializer.SetUp();

            // Default route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{*id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}