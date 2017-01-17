using System.Configuration;
using Autofac;
using Saritasa.Tools.Messages.Commands;

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
            var commandPipeline = CommandPipeline.CreateDefaultPipeline(Container.Resolve,
                System.Reflection.Assembly.GetAssembly(typeof(Domain.Users.Entities.User)));
            var connectionString = ConfigurationManager.ConnectionStrings["AppDbContext"];
            commandPipeline.AppendMiddlewares(
                new Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware(
                    new Saritasa.Tools.Messages.Common.Repositories.AdoNetMessageRepository(
                        System.Data.Common.DbProviderFactories.GetFactory(connectionString.ProviderName),
                        connectionString.ConnectionString,
                        Tools.Messages.Common.Repositories.AdoNetMessageRepository.Dialect.SqlServer
                    )
                )
            );
            builder = new ContainerBuilder();
            builder.RegisterInstance(commandPipeline).AsImplementedInterfaces().SingleInstance();

            builder.Update(Container);
        }
    }
}