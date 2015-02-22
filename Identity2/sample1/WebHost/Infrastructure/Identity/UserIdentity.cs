using System;
using System.Linq;
using Microsoft.AspNet.Identity;

namespace Identity2.Sample1.WebHost.Infrastructure.Identity
{
    public class UserIdentity : IUser<int>
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public UserIdentity()
        {
        }

        public UserIdentity(string username)
        {
            if (string.IsNullOrEmpty("username"))
                throw new ArgumentNullException("username");
            UserName = username;
        }
    }
}