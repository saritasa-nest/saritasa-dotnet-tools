namespace Saritasa.BoringWarehouse.Web
{
    using System.Configuration;
    using System.Web.Mvc;

    using Autofac;
    using Autofac.Integration.Mvc;

    /// <summary>
    /// Dependency injection configuration.
    /// </summary>
    public class DiConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();

            // register MVC controllers
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // register web abstractions like HttpContextBase
            builder.RegisterModule<AutofacWebTypesModule>();

            // enable property injection in view pages
            builder.RegisterSource(new ViewRegistrationSource());

            // enable property injection into action filters
            builder.RegisterFilterProvider();

            // other bindings
            builder.RegisterType<DataAccess.AppDbContext>().AsSelf();
            builder.RegisterType<DataAccess.AppUnitOfWork>().AsImplementedInterfaces();
            builder.RegisterType<DataAccess.AppUnitOfWorkFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<Domain.Users.Queries.UserQueries>().AsSelf();
            builder.RegisterType<Domain.Products.Queries.ProductQueries>().AsSelf();
            builder.RegisterType<Domain.Products.Queries.CompanyQueries>().AsSelf();

            // make container
            var container = builder.Build();

            // command pipeline
            var commandPipeline = Tools.Commands.CommandPipeline.CreateDefaultPipeline(container.Resolve,
                System.Reflection.Assembly.GetAssembly(typeof(Domain.Users.Entities.User)));
            var connectionString = ConfigurationManager.ConnectionStrings["AppDbContext"];
            commandPipeline.AppendMiddlewares(
                new Saritasa.Tools.Messages.PipelineMiddlewares.RepositoryMiddleware(
                    new Saritasa.Tools.Messages.Repositories.AdoNetMessageRepository(
                        System.Data.Common.DbProviderFactories.GetFactory(connectionString.ProviderName),
                        connectionString.ConnectionString,
                        Saritasa.Tools.Messages.Repositories.AdoNetMessageRepository.Dialect.SqlServer
                    )
                )
            );
            builder = new ContainerBuilder();
            builder.RegisterInstance(commandPipeline).AsImplementedInterfaces().SingleInstance();

            // query pipeline

            // set the dependency resolver to be Autofac
            builder.Update(container);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
