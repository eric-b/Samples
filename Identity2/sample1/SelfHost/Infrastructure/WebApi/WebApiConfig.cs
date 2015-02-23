using System;
using System.Linq;
using System.Web.Http;

namespace Identity2.Sample1.SelfHost.Infrastructure.WebApi
{
    public static class WebApiConfig
    {
        // Cf. http://www.asp.net/web-api/overview/advanced/configuring-aspnet-web-api
        public static void Configure(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}