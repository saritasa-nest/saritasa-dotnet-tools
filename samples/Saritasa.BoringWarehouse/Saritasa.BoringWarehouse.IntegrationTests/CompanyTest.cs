using System.Configuration;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Saritasa.BoringWarehouse.Domain;
using Saritasa.BoringWarehouse.Domain.Products.Commands;
using Saritasa.BoringWarehouse.Domain.Products.Queries;
using Saritasa.Tools.Commands;

namespace Saritasa.BoringWarehouse.IntegrationTests
{
    public class CompanyTest
    {
        private IContainer container;
        private ICommandPipeline commandPipeline;

        [SetUp]
        public void SetUp()
        {
            SetUpAutofac();
            commandPipeline = container.Resolve<ICommandPipeline>();
        }

        private void SetUpAutofac()
        {
            var builder = new ContainerBuilder();

            // other bindings
            builder.RegisterType<DataAccess.AppDbContext>().AsSelf();
            builder.RegisterType<DataAccess.AppUnitOfWork>().AsImplementedInterfaces();
            builder.RegisterType<DataAccess.AppUnitOfWorkFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<Domain.Users.Queries.UserQueries>().AsSelf();
            builder.RegisterType<Domain.Products.Queries.ProductQueries>().AsSelf();
            builder.RegisterType<Domain.Products.Queries.CompanyQueries>().AsSelf();

            container = builder.Build();

            // command pipeline
            var defaultPipeline = Tools.Commands.CommandPipeline.CreateDefaultPipeline(container.Resolve,
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

            builder.Update(container);
        }

        [TestCase]
        public void TestCompanyCreation()
        {
            using (var uow = container.Resolve<IAppUnitOfWork>())
            {
                var query = new CompanyQueries(uow);
                var count1 = query.GetAll().Count();

                var command = new CreateCompanyCommand();
                command.CreatedByUserId = 1;
                command.Name = "Test Company";

                commandPipeline.Handle(command);

                var count2 = query.GetAll().Count();
                Assert.AreEqual(count1 + 1, count2, "number of companies");
            }
        }
    }
}