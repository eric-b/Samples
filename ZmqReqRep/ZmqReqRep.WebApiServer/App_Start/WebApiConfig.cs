using System;
using System.Linq;
using System.Web.Http;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using ZmqReqRep.Client;

namespace ZmqReqRep.WebApiServer
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            SetupIoc();
        }

        private static void SetupIoc()
        {
            var container = new Container();
            container.RegisterSingle<ClientFactory>(() => new ClientFactory("tcp://localhost:1040"));
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }
    }
}