using System;
using System.Linq;
using System.Threading.Tasks;

namespace Identity2.Sample1.WebHost.Infrastructure.Ldap
{
    public class FakeLdapAuthClient : ILdapAuthClient
    {
        public Task<bool> ValidateUserAsync(string username, string password)
        {
            return Task.FromResult(true);
        }
    }
}