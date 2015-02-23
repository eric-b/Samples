using System;
using System.Linq;
using Identity2.Sample1.Shared.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using Owin;

[assembly: OwinStartup(typeof(Identity2.Sample1.WebHost.OwinStartup))]

namespace Identity2.Sample1.WebHost
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // Mise en place du middleware owin pour gérer le cookie d'authentification
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                TicketDataFormat = new TicketDataFormat(
                    new MachineKeyDataProtector(
                        "Microsoft.Owin.Security.Cookies.CookieAuthenticationMiddleware",
                        DefaultAuthenticationTypes.ApplicationCookie,
                        "v1"))
            });
        }
    }
}