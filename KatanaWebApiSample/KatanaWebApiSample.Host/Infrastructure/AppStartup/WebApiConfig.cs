using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using CacheCow.Server;

namespace KatanaWebApiSample.Host.Infrastructure.AppStartup
{
    public static class WebApiConfig
    {
        /// <summary>
        /// Set up WebApi
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {
            // Default route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Replaces IAssembliesResolver to allow resolution 
            // of controllers in other assemblies (not necessarily 
            // already loaded into the current AppDomain).
            config.Services.Replace(typeof(IAssembliesResolver), new CustomAssembliesResolver());

            // Cache (depends on CacheCow package)
            config.MessageHandlers.Add(new CachingHandler(config));
        }
    }
}