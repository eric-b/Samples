using System;
using System.Linq;
using System.Threading.Tasks;

namespace Identity2.Sample1.WebHost.Infrastructure.Ldap
{
    public interface ILdapAuthClient
    {
        Task<bool> ValidateUserAsync(string username, string password);
    }
}