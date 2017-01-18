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

            // other bindings
            builder.RegisterType<DataAccess.AppDbContext>().AsSelf();
            builder.RegisterType<DataAccess.AppUnitOfWorkFactory>().AsSelf().AsImplementedInterfaces();
            builder.Register(c => c.Resolve<DataAccess.AppUnitOfWorkFactory>().Create()).AsImplementedInterfaces();
            builder.RegisterType<Domain.Users.Queries.UserQueries>().AsSelf();
            builder.RegisterType<Domain.Products.Queries.ProductQueries>().AsSelf();
            builder.RegisterType<Domain.Products.Queries.CompanyQueries>().AsSelf();

            // command pipeline
            builder.Register<ICommandPipeline>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    var commandPipeline = CommandPipeline.CreateDefaultPipeline(context.Resolve,
                        System.Reflection.Assembly.GetAssembly(typeof(Domain.Users.Entities.User)));
                    var connectionString = ConfigurationManager.ConnectionStrings["AppDbContext"];
                    commandPipeline.AppendMiddlewares(
                        new Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware(
                            new Tools.Messages.Common.Repositories.AdoNetMessageRepository(
                                System.Data.Common.DbProviderFactories.GetFactory(connectionString.ProviderName),
                                connectionString.ConnectionString,
                                Tools.Messages.Common.Repositories.AdoNetMessageRepository.Dialect.SqlServer
                            )
                        )
                    );
                    commandPipeline.UseInternalResolver();

                    return commandPipeline;
                }).SingleInstance();

            return builder;
        }
    }
}
