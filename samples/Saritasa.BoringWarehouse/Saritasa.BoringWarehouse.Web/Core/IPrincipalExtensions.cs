using System.Linq;
using System.Security.Principal;
using Saritasa.BoringWarehouse.Domain.Users.Entities;

namespace Saritasa.BoringWarehouse.Web.Core
{
    /// <summary>
    /// Extensions for <see cref="IPrincipal" />.
    /// </summary>
    public static class IPrincipalExtensions
    {
        public static bool IsInRoles(this IPrincipal principal, params UserRole[] roles)
        {
            return roles.Any(r => principal.IsInRole(r.ToString()));
        }
    }
}
