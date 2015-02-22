using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Identity2.Sample1.WebHost.Infrastructure.Identity
{
    public class ApplicationSignInManager : SignInManager<UserIdentity, int>
    {
        public ApplicationSignInManager(UserManager<UserIdentity, int> userManager, IAuthenticationManager authManager) : base(userManager, authManager)
        {
        }

        public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            if (UserManager == null)
                return SignInStatus.Failure;
            var user = new UserIdentity(userName);
          
            bool isAuth = await UserManager.CheckPasswordAsync(user, password);
            if (!isAuth)
                return SignInStatus.Failure;

            user = await UserManager.FindByNameAsync(userName); 
            await base.SignInAsync(user, isPersistent, false);
            return SignInStatus.Success;
        }
    }
}