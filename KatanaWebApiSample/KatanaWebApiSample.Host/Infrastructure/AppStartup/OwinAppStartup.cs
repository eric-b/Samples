using System;
using System.Web.Http;
using Owin;

namespace KatanaWebApiSample.Host.Infrastructure.AppStartup
{
    /// <summary>
    /// <para>Initialize the application in Owin.</para>
    /// </summary>
    internal class OwinAppStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            #region WebApi (Web Framework in OWIN terminology)
            
            HttpConfiguration webApiConfig = new HttpConfiguration();
            webApiConfig.DependencyResolver = IocInitializer.SetUp();
            WebApiConfig.Register(webApiConfig);
            appBuilder.UseWebApi(webApiConfig);
            
            #endregion
        }
    }
}