namespace Saritasa.BoringWarehouse.Web.Core
{
    using System.Linq;
    using System.Security.Principal;

    using Domain.Users.Entities;

    public static class IPrincipalExtensions
    {
        public static bool IsInRoles(this IPrincipal principal, params UserRole[] roles)
        {
            return roles.Any(r => principal.IsInRole(r.ToString()));
        }
    }
}