using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;

namespace ZergRushCo.Todosya.Web
{
    /// <summary>
    /// Dependency injection configuration.
    /// </summary>
    public class DiConfig
    {
        /// <summary>
        /// Configures dependency injection container.
        /// </summary>
        public static void Register()
        {
            var builder = new ContainerBuilder();

            // Register MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // Enable property injection into action filters.
            builder.RegisterFilterProvider();

            builder.Register(
                c => new Microsoft.AspNet.Identity.Owin.SignInManager<Domain.UserContext.Entities.User, string>(
                    c.Resolve<Domain.UserContext.Services.AppUserManager>(),
                    HttpContext.Current.GetOwinContext().Authentication));
            ZergRushCo.Todosya.Infrastructure.DiConfig.Setup(builder);

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
