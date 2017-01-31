namespace Unosquare.Swan.AspNetCore
{
    using Microsoft.AspNetCore.Identity;
    using Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a basic RoleStore for AspNetCore Identity
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Identity.IRoleStore{Unosquare.Swan.AspNetCore.Models.ApplicationRole}" />
    public class BasicRoleStore : IRoleStore<ApplicationRole>
    {
        private readonly List<ApplicationRole> _roles;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRoleStore"/> class.
        /// </summary>
        public BasicRoleStore()
        {
            _roles = new List<ApplicationRole>();
        }

        public Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            _roles.Add(role);

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            var match = _roles.FirstOrDefault(r => r.RoleId == role.RoleId);
            if (match != null)
            {
                match.RoleName = role.RoleName;

                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed());
            }
        }

        public Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            var match = _roles.FirstOrDefault(r => r.RoleId == role.RoleId);
            if (match != null)
            {
                _roles.Remove(match);

                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed());
            }
        }

        public Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var role = _roles.FirstOrDefault(r => r.RoleId == roleId);

            return Task.FromResult(role);
        }

        public Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var role = _roles.FirstOrDefault(r => r.RoleName == normalizedRoleName);

            return Task.FromResult(role);
        }

        public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.RoleId);
        }

        public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.RoleName);
        }

        public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.RoleName);
        }

        public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            role.RoleName = roleName;

            return Task.FromResult(true);
        }

        public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.RoleName = normalizedName;

            return Task.FromResult(true);
        }

        public void Dispose() { }
    }
}
