using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleWebApi.Infrastructure;
using SimpleWebApi.Infrastructure.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace SimpleWebApi
{
    public static class SimpleInjectorConfig
    {
        public static void SetUp(HttpConfiguration config)
        {
            // Cf. https://simpleinjector.readthedocs.org/en/latest/webapiintegration.html#basic-setup

            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();

            container.RegisterSingleton<ILogger, Logger>();

            container.RegisterCollection(typeof(IActionFilter<>), new Assembly[] { typeof(IActionFilter<>).Assembly });

            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

            container.Verify();

            config.Filters.Add(new ActionFilterDispatcher(container.GetAllInstances));
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }
    }
}