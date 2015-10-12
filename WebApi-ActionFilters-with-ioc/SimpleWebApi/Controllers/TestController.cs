using SimpleWebApi.Infrastructure.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SimpleWebApi.Controllers
{
    public class TestController : ApiController
    {
        [MeasureTimeFilter("some metadata")]
        public string Get()
        {
            return "Test OK";
        }
    }
}
