using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Identity2.Sample1.WebHost
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "FreeView", action = "Index", id = UrlParameter.Optional });
        }
    }
}