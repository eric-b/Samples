using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Identity2.Sample1.WebHost.Infrastructure.Identity
{
    public class FakeUserStore : IUserStore<UserIdentity, int>, IUserRoleStore<UserIdentity, int>
    {
        private readonly List<UserIdentity> _users = new List<UserIdentity>();

        public void Dispose()
        {
        }

        public Task AddToRoleAsync(UserIdentity user, string roleName)
        {
            return Task.FromResult<object>(null);
        }

        public Task RemoveFromRoleAsync(UserIdentity user, string roleName)
        {
            return Task.FromResult<object>(null);
        }

        public Task<IList<string>> GetRolesAsync(UserIdentity user)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<bool> IsInRoleAsync(UserIdentity user, string roleName)
        {
            return Task.FromResult(true);
        }

        public Task CreateAsync(UserIdentity user)
        {
            user.Id = _users.Count;
            _users.Add(user);
            return Task.FromResult<object>(null);
        }

        public Task UpdateAsync(UserIdentity user)
        {
            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(UserIdentity user)
        {
            return Task.FromResult<object>(null);
        }

        public Task<UserIdentity> FindByIdAsync(int userId)
        {
            return Task.FromResult(_users.FirstOrDefault(t => t.Id == userId));
        }

        public Task<UserIdentity> FindByNameAsync(string userName)
        {
            return Task.FromResult(_users.FirstOrDefault(t => t.UserName == userName));
        }
    }
}