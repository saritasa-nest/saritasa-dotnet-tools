using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Saritasa.Tools.Events;
using Saritasa.Tools.Exceptions;
using ZergRushCo.Todosya.Domain.Users.Commands;

namespace ZergRushCo.Todosya.Tests
{
    /// <summary>
    /// Tests for user module.
    /// </summary>
    [TestFixture]
    public class UsersTests
    {
        [Test]
        public void Creating_user_with_existing_email_should_generate_exception()
        {
            var uowFactory = new AppTestUnitOfWorkFactory();

            var userHandlers = new Domain.Users.Handlers.UserHandlers();
            var registerUserCommand = new RegisterUserCommand()
            {
                FirstName = "Ivan",
                LastName = "Ivanov",
                Password = "111111",
                ConfirmPassword = "111111",
                Email = "test@saritasa.com",
            };
            userHandlers.HandleRegisterUser(registerUserCommand, uowFactory, EventPipeline.Empty);

            bool isExceptionFired = false;
            try
            {
                userHandlers.HandleRegisterUser(registerUserCommand, uowFactory, EventPipeline.Empty);
            }
            catch (DomainException)
            {
                isExceptionFired = true;
            }
            Assert.That(isExceptionFired, Is.True);
        }

        [Test]
        public void User_should_not_be_able_to_update_email_to_existing_one()
        {
            var uowFactory = new AppTestUnitOfWorkFactory();

            var userHandlers = new Domain.Users.Handlers.UserHandlers();
            var registerUserCommand1 = new RegisterUserCommand()
            {
                FirstName = "Ivan",
                LastName = "Ivanov",
                Password = "111111",
                ConfirmPassword = "111111",
                Email = "test@saritasa.com",
            };
            userHandlers.HandleRegisterUser(registerUserCommand1, uowFactory, EventPipeline.Empty);
            var registerUserCommand2 = new RegisterUserCommand()
            {
                FirstName = "Stepan",
                LastName = "Stepanov",
                Password = "111111",
                ConfirmPassword = "111111",
                Email = "test+ivanov1@saritasa.com",
            };
            userHandlers.HandleRegisterUser(registerUserCommand2, uowFactory, EventPipeline.Empty);

            bool isExceptionFired = false;
            try
            {
                var usersQueries = new Domain.Users.Queries.UsersQueries(uowFactory.Create());
                var user = usersQueries.GetByEmail("test@saritasa.com");
                var updateUserCommand = new UpdateUserCommand()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Password = "111112",
                    ConfirmPassword = "111112",
                    Email = "test+ivanov1@saritasa.com",
                };
                userHandlers.HandleUpdateUser(updateUserCommand, uowFactory);
            }
            catch (DomainException)
            {
                isExceptionFired = true;
            }
            Assert.That(isExceptionFired, Is.True);
        }
    }
}
