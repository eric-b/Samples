using System;
using System.DirectoryServices;
using System.Linq;
using System.Threading.Tasks;

namespace Identity2.Sample1.WebHost.Infrastructure.Ldap
{
    public class AdAuthClient : ILdapAuthClient
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Example: "LDAP://ad.domain.com"</param>
        public AdAuthClient(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");
            _connectionString = connectionString;
        }

        Task<bool> ILdapAuthClient.ValidateUserAsync(string username, string password)
        {
            const bool accessDenied = false;
            var entryBase = new DirectoryEntry(_connectionString, username, password);
            try
            {
                //Bind to the native AdsObject to force authentication.			
                Object obj = entryBase.NativeObject;
                return Task.FromResult(obj != null);
            }
            catch (DirectoryServicesCOMException)
            {
                return Task.FromResult(accessDenied);
            }
            finally
            {
                entryBase.Dispose();
            }
        }
    }
}