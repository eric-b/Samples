using System;
using System.Linq;
using System.Web.Http;

namespace ZmqReqRep.WebApiServer
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}