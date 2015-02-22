using System;
using System.Linq;
using System.Web.Mvc;

namespace Identity2.Sample1.WebHost.Controllers
{
    [AllowAnonymous]
    public class FreeViewController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}