using System;
using System.Linq;
using System.Web.Http;
using Identity2.Sample1.SelfHost.Infrastructure.WebApi;
using Identity2.Sample1.Shared.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using Owin;

namespace Identity2.Sample1.SelfHost.Infrastructure.Owin
{
    internal class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            #region WebApi (Web Framework in OWIN terminology)
            
            HttpConfiguration webApiConfig = new HttpConfiguration();
            WebApiConfig.Configure(webApiConfig);
            
            #endregion
            
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                TicketDataFormat = new TicketDataFormat(
                    new MachineKeyDataProtector(
                        "Microsoft.Owin.Security.Cookies.CookieAuthenticationMiddleware", 
                        DefaultAuthenticationTypes.ApplicationCookie, 
                        "v1"))
            });
            app.UseWebApi(webApiConfig);
            
            app.SetLoggerFactory(new Infrastructure.Owin.LoggerFactory());
        }
    }
}
