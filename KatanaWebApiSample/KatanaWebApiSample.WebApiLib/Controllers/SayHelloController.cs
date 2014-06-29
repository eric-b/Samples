using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace KatanaWebApiSample.WebApiLib.Controllers
{
    /// <summary>
    /// My very useful WebApi...
    /// </summary>
    public class SayHelloController : ApiController
    {
        private readonly IMyService _myService;

        // myService is injected by the IoC container
        public SayHelloController(IMyService myService)
        {
            _myService = myService;
        }

        // GET /api/SayHello/Smith
        public string GetWithId(string id)
        {
            // emulate some heavy processing to demonstrate use of output caching
            Task.Delay(1000).Wait(); 

            // some very useful app domain logic...
            return _myService.SayHello(id);
        }

        // GET /api/SayHello
        public string Get()
        {
            // emulate some heavy processing to demonstrate use of output caching
            Task.Delay(1000).Wait();

            // some very useful app domain logic...
            return _myService.SayHello("World !");
        }
    }
}