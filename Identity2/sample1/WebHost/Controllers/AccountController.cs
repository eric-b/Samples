using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Identity2.Sample1.WebHost.Infrastructure.Identity;
using Identity2.Sample1.WebHost.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Identity2.Sample1.WebHost.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationUserManager _userManager;

        private readonly IAuthenticationManager _authManager;
     
        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                if (_authManager != null)
                    return _authManager;

                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public AccountController(ApplicationUserManager userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Constructor for unit tests.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="authenticationManager"></param>
        internal AccountController(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) : this(userManager)
        {
            _authManager = authenticationManager;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var signinManager = new ApplicationSignInManager(_userManager, AuthenticationManager))
            {
                var result = await signinManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.Success:
                        return RedirectToLocal(returnUrl);
                    default:
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "FreeView");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "FreeView");
        }
    }
}