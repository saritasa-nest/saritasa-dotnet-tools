using Saritasa.BoringWarehouse.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace Saritasa.BoringWarehouse.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DiConfig.Register();
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Context.User == null)
            {
                // Not logined
                return;
            }
            var formsIdentity = Context.User.Identity as FormsIdentity;
            if (formsIdentity == null)
            {
                return;
            }
            try
            {
                TicketUserData userData = TicketUserData.FromString(formsIdentity.Ticket.UserData);
                if (Context.User != null)
                {
                    Context.User = new GenericPrincipal(Context.User.Identity, new[] { userData.UserRole.ToString() });
                }
            }
            catch
            {
                return;
            }
        }
    }
}
