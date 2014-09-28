using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ServerConsole.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Allows Cross Origin Resource Sharing
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

            // Supports only JSON
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: null,
                // Force content-type: text/html
                handler: new Infrastructure.DelegatingHandlers.JsonForIEDelegateHandler(config));
        }
    }
}