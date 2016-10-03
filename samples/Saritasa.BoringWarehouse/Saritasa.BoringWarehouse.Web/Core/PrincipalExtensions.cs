using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Saritasa.BoringWarehouse.Domain.Users.Entities;

namespace Saritasa.BoringWarehouse.Web.Core
{
    public static class PrincipalExtensions
    {
        public static bool IsInRoles(this IPrincipal principal, params UserRole[] roles)
        {
            return roles.Any(r => principal.IsInRole(r.ToString()));
        }
    }
}
