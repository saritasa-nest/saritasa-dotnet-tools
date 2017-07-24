using Saritasa.Tools.Messages.Common;

namespace Saritasa.BoringWarehouse.Infrastructure
{
    using System.Configuration;

    using Tools.Messages.Abstractions;

    using Autofac;
    using Tools.Messages.Commands;

    /// <summary>
    /// Dependency injection configuration.
    /// </summary>
    public class CommonDIConfig
    {
        /// <summary>
        /// Prepares Autofac container builder with common services.
        /// </summary>
        /// <returns>Autofac container builder.</returns>
        public static ContainerBuilder CreateBuilder()
        {
            var builder = new ContainerBuilder();

            // Bindings.
            builder.RegisterType<DataAccess.AppDbContext>().AsSelf();
            builder.RegisterType<DataAccess.AppUnitOfWorkFactory>().AsSelf().AsImplementedInterfaces();
            builder.Register(c => c.Resolve<DataAccess.AppUnitOfWorkFactory>().Create()).AsImplementedInterfaces();
            builder.RegisterType<Domain.Users.Queries.UserQueries>().AsSelf();
            builder.RegisterType<Domain.Products.Queries.ProductQueries>().AsSelf();
            builder.RegisterType<Domain.Products.Queries.CompanyQueries>().AsSelf();

            // Repository for messages.
            var connectionString = ConfigurationManager.ConnectionStrings["AppDbContext"];
            var adoNetRepositoryMiddleware = new Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware(
                new Tools.Messages.Common.Repositories.AdoNetMessageRepository(
                    System.Data.Common.DbProviderFactories.GetFactory(connectionString.ProviderName),
                    connectionString.ConnectionString,
                    Tools.Messages.Common.Repositories.AdoNetMessageRepository.Dialect.SqlServer
                )
            );

            // Command pipeline.
            var messagePipelineContainer = new SimpleMessagePipelineContainer();
            messagePipelineContainer.AddCommandPipeline()
                .UseDefaultMiddlewares(System.Reflection.Assembly.GetAssembly(typeof(Domain.Users.Entities.User)))
                .AddMiddleware(adoNetRepositoryMiddleware);
            builder.RegisterInstance(messagePipelineContainer).As<IMessagePipelineContainer>().SingleInstance();
            builder.RegisterType<DefaultPipelineService>().As<IPipelineService>();

            return builder;
        }
    }
}
