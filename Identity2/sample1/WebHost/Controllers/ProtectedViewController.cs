using System;
using System.Linq;
using System.Web.Mvc;

namespace Identity2.Sample1.WebHost.Controllers
{
    [Authorize]
    public class ProtectedViewController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}