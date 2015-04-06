using System;
using System.Linq;
using System.Web.Http;
using ZmqReqRep.Client;

namespace ZmqReqRep.WebApiServer.Controllers
{
    public class RequestController : ApiController
    {
        private readonly ClientFactory _factory;

        public RequestController(ClientFactory clientFactory)
        {
            _factory = clientFactory;
        }

        public string Get(string id)
        {
            IReqSocket client = _factory.Create();
            try
            {
                return client.SendRequest(id);
            }
            finally
            {
                _factory.Release(client);
            }
        }
    }
}