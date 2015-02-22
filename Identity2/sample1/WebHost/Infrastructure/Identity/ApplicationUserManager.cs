using System;
using System.Linq;
using System.Threading.Tasks;
using Identity2.Sample1.WebHost.Infrastructure.Ldap;
using Microsoft.AspNet.Identity;

namespace Identity2.Sample1.WebHost.Infrastructure.Identity
{
    public class ApplicationUserManager : UserManager<UserIdentity, int>
    {
        private readonly ILdapAuthClient _ldapAuth;
        private readonly IUserRoleStore<UserIdentity, int> _store;

        public ApplicationUserManager(IUserRoleStore<UserIdentity, int> store, ILdapAuthClient ldapAuth) : base(store)
        {
            if (store == null)
                throw new ArgumentNullException("store");
            if (ldapAuth == null)
                throw new ArgumentNullException("ldapAuth");
            _ldapAuth = ldapAuth;
            _store = store;
        }

        public override async Task<bool> CheckPasswordAsync(UserIdentity user, string password)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            
            var authResult = await _ldapAuth.ValidateUserAsync(user.UserName, password);

            if (!authResult)
                return false;

            UserIdentity existingUser = await Store.FindByNameAsync(user.UserName);
            if (existingUser == null)
            {
                await _store.CreateAsync(user);
            }

            return true;
        }
    }
}