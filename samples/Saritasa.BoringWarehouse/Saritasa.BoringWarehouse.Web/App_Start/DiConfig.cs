using Saritasa.BoringWarehouse.Infrastructure;

namespace Saritasa.BoringWarehouse.Web
{
    using System.Web.Mvc;

    using Autofac;
    using Autofac.Integration.Mvc;

    /// <summary>
    /// Dependency injection configuration.
    /// </summary>
    public class DIConfig
    {
        public static void Register()
        {
            var builder = CommonDIConfig.CreateBuilder();

            // register MVC controllers
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // register web abstractions like HttpContextBase
            builder.RegisterModule<AutofacWebTypesModule>();

            // enable property injection in view pages
            builder.RegisterSource(new ViewRegistrationSource());

            // enable property injection into action filters
            builder.RegisterFilterProvider();

            // set the dependency resolver to be Autofac
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
