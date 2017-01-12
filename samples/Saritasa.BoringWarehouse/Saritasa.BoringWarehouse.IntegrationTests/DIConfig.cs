using System.Configuration;
using Autofac;

namespace Saritasa.BoringWarehouse.IntegrationTests
{
    internal static class DIConfig
    {
        public static IContainer Container { get; private set; }

        public static void Initialize()
        {
            var builder = new ContainerBuilder();

            // other bindings
            builder.RegisterType<DataAccess.AppDbContext>().AsSelf();
            builder.RegisterType<DataAccess.AppUnitOfWork>().AsImplementedInterfaces();
            builder.RegisterType<DataAccess.AppUnitOfWorkFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<Domain.Users.Queries.UserQueries>().AsSelf();
            builder.RegisterType<Domain.Products.Queries.ProductQueries>().AsSelf();
            builder.RegisterType<Domain.Products.Queries.CompanyQueries>().AsSelf();

            Container = builder.Build();

            // command pipeline
            var defaultPipeline = Tools.Commands.CommandPipeline.CreateDefaultPipeline(Container.Resolve,
                System.Reflection.Assembly.GetAssembly(typeof(Domain.Users.Entities.User)));
            var connectionString = ConfigurationManager.ConnectionStrings["AppDbContext"];
            defaultPipeline.AppendMiddlewares(
                new Saritasa.Tools.Messages.PipelineMiddlewares.RepositoryMiddleware(
                    new Saritasa.Tools.Messages.Repositories.AdoNetMessageRepository(
                        System.Data.Common.DbProviderFactories.GetFactory(connectionString.ProviderName),
                        connectionString.ConnectionString,
                        Saritasa.Tools.Messages.Repositories.AdoNetMessageRepository.Dialect.SqlServer
                    )
                )
            );
            builder = new ContainerBuilder();
            builder.RegisterInstance(defaultPipeline).AsImplementedInterfaces().SingleInstance();

            builder.Update(Container);
        }
    }
}