using System;
using System.Linq;
using System.Web.Http;

namespace Identity2.Sample1.SelfHost.Controllers
{
    [Authorize]
    public class ProtectedValuesController : ApiController
    {
        public string[] Get()
        {
            return new string[]
            {
                "1",
                "2",
                "3"
            };
        }
    }
}