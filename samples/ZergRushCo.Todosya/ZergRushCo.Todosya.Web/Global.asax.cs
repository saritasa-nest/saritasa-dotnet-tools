using System.Web.Mvc;
using System.Web.Routing;

namespace ZergRushCo.Todosya.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            DiConfig.Register();
        }
    }
}
