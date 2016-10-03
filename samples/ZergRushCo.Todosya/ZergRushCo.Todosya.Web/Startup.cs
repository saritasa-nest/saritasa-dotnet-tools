using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ZergRushCo.Todosya.Web.Startup))]
namespace ZergRushCo.Todosya.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
