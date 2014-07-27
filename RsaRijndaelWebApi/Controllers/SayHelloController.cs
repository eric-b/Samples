using System;
using System.Linq;
using System.Web.Http;
using RsaRijndaelWebApi.Infrastructure.ActionFilters;

namespace RsaRijndaelWebApi.Controllers
{
    [InternalActionFilterAttribute]
    public class SayHelloController : ApiController
    {
        // GET /api/SayHello/id
        public string Get(string id)
        {
            return string.Format("Hello {0}", id);
        }
    }
}