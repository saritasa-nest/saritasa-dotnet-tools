using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Saritasa.BoringWarehouse.Infrastructure;

namespace Saritasa.BoringWarehouse.Web
{

    /// <summary>
    /// Dependency injection configuration.
    /// </summary>
    public class DIConfig
    {
        public static void Register()
        {
            var builder = CommonDIConfig.CreateBuilder();

            // Register MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // Enable property injection into action filters.
            builder.RegisterFilterProvider();

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
