using System;
using System.Linq;
using System.Web.Http;
using Owin;

namespace RsaRijndaelWebApi.Infrastructure.AppStartup
{
    internal class OwinAppStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration webApiConfig = new HttpConfiguration();
            WebApiConfig.Register(webApiConfig);
            appBuilder.UseWebApi(webApiConfig);
        }
    }
}