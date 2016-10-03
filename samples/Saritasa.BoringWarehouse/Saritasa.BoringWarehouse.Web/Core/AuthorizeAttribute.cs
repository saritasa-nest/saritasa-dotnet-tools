using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Saritasa.BoringWarehouse.Domain.Users.Entities;

namespace Saritasa.BoringWarehouse.Web.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        public AuthorizeAttribute()
        {
        }

        public AuthorizeAttribute(params UserRole[] roles)
        {
            if (roles != null)
            {
                Roles = string.Join(",", roles.Select(r => r.ToString()));
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new HttpStatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
