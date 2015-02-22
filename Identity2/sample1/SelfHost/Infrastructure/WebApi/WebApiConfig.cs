using System;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace Identity2.Sample1.SelfHost.Infrastructure.WebApi
{
    public static class WebApiConfig
    {
        // Cf. http://www.asp.net/web-api/overview/advanced/configuring-aspnet-web-api
        public static void Configure(HttpConfiguration config)
        {
            // TODO : utile ?
            config.Filters.Add(new HostAuthenticationFilter(DefaultAuthenticationTypes.ApplicationCookie)); // pour test...

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}