using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ZergRushCo.Todosya.Web.Startup))]
namespace ZergRushCo.Todosya.Web
{
    /// <summary>
    /// Owin startup class.
    /// </summary>
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
