using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace ServerConsole.Controllers
{
    /// <summary>
    /// Simulates a static HTML page containing a file upload form.
    /// </summary>
    public class UploadPageController : ApiController
    {
        private readonly DemoContext _demoContext;

        public UploadPageController(DemoContext demoContext)
        {
            _demoContext = demoContext;
        }

        public HttpResponseMessage Get()
        {
            return Get("app");
        }

        public HttpResponseMessage Get(string id)
        {
            var responseMsg = Request.CreateResponse(System.Net.HttpStatusCode.OK);

            string name = "upload-app.html";
            if (id != null)
            {
                switch (id.ToLower())
                {
                    case "form":
                        name = "upload-form.html";
                        break;
                    case "app":
                        name = "upload-app.html";
                        break;
                    case "result":
                        name = "result.html";
                        break;
                }
            }
            
            string html = Helper
                .GetResourceAsString(name)
                .Replace("{server1-base-address}", string.Format("http://localhost:{0}/", _demoContext.Server1Port))
                .Replace("{server2-base-address}", string.Format("http://localhost:{0}/", _demoContext.Server2Port));

            responseMsg.Content = new StringContent(html, Encoding.UTF8, "text/html"); 
            return responseMsg;
        }
    }
}