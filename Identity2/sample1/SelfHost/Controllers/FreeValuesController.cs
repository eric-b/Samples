using System;
using System.Linq;
using System.Web.Http;

namespace Identity2.Sample1.SelfHost.Controllers
{
    [AllowAnonymous]
    public class FreeValuesController : ApiController
    {
        public string[] Get()
        {
            return new string[]
            {
                "A",
                "B",
                "C"
            };
        }
    }
}