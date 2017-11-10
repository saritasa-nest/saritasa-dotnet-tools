using System.Web.Mvc;
using System.Web.Routing;

namespace ZergRushCo.Todosya.Web
{
    /// <summary>
    /// Global application object.
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            var container = DiConfig.Register();
            DebugEndpointConfig.Register(container);
        }
    }
}
