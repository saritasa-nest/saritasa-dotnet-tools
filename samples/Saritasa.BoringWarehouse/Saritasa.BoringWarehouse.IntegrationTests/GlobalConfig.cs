using Autofac;
using NUnit.Framework;
using Saritasa.BoringWarehouse.Domain;
using Saritasa.BoringWarehouse.Domain.Users.Commands;
using Saritasa.BoringWarehouse.Domain.Users.Entities;
using Saritasa.Tools.Commands;

namespace Saritasa.BoringWarehouse.IntegrationTests
{
    [SetUpFixture]
    public class GlobalConfig
    {
        public static int AdminId { get; private set; }

        private ICommandPipeline commandPipeline;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DIConfig.Initialize();

            var container = DIConfig.Container;
            commandPipeline = container.Resolve<ICommandPipeline>();
            using (var uow = container.Resolve<IAppUnitOfWork>())
            {
                CreateAdmin(uow);
            }
        }

        private void CreateAdmin(IAppUnitOfWork uow)
        {
            var command = new CreateUserCommand
            {
                FirstName = "Admin",
                LastName = "Admin",
                Email = "admin@example.com",
                Password = "secret",
                Role = UserRole.Admin
            };
            commandPipeline.Handle(command);

            AdminId = command.UserId;
        }
    }
}