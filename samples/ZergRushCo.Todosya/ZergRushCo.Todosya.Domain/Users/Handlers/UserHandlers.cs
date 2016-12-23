using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Domain.Exceptions;
using ZergRushCo.Todosya.Domain.Users.Commands;
using ZergRushCo.Todosya.Domain.Users.Entities;
using ZergRushCo.Todosya.Domain.Users.Events;

namespace ZergRushCo.Todosya.Domain.Users.Handlers
{
    /// <summary>
    /// User commands handlers.
    /// </summary>
    [CommandHandlers]
    public class UserHandlers
    {
        readonly ILogger logger = AppLogging.CreateLogger<UserHandlers>();

        /// <summary>
        /// Handle user registration.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="uowFactory">Application unit of work factory.</param>
        /// <param name="eventsPipeline">Events pipeline.</param>
        public void HandleRegisterUser(RegisterUserCommand command, IAppUnitOfWorkFactory uowFactory,
            IEventPipeline eventsPipeline)
        {
            using (var uow = uowFactory.Create())
            {
                var email = command.Email.ToLowerInvariant().Trim();
                if (uow.UserRepository.Any(x => x.Email == email))
                {
                    throw new DomainException("The user with the same email already exists");
                }

                var user = new User()
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    PasswordHash = command.Password,
                    City = command.City,
                    BirthDay = command.BirthDay,
                    Country = command.Country,
                    Email = command.Email,
                    UserName = email,
                };
                uow.UserRepository.Add(user);
                uow.SaveChanges();

                eventsPipeline.Raise(new UserCreatedEvent()
                {
                    User = user,
                });

                command.UserId = user.Id;
                logger.LogInformation($"User {user.FirstName} {user.LastName} with id {user.Id} has been registered.");
            }
        }

        /// <summary>
        /// Handle user update.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="uowFactory">Application unit of work factory.</param>
        public void HandleUpdateUser(UpdateUserCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            var email = command.Email.ToLowerInvariant().Trim();
            using (var uow = uowFactory.Create())
            {
                var user = uow.UserRepository.FirstOrDefault(u => u.Id == command.UserId);
                if (user == null)
                {
                    throw new NotFoundException("User not found");
                }
                if (user.Email != email && uow.UserRepository.Any(x => x.Email == email))
                {
                    throw new DomainException("The user with the same email already exists");
                }

                user.Email = command.Email;
                user.UserName = command.Email;
                user.FirstName = command.FirstName;
                user.LastName = command.LastName;
                user.Country = command.Country;
                user.City = command.City;
                user.UpdatedAt = DateTime.Now;

                uow.SaveChanges();
                command.HasPassword = string.IsNullOrEmpty(user.PasswordHash) == false;
            }
        }

        /// <summary>
        /// Handle user update.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="uowFactory">Application unit of work factory.</param>
        public void HandleRawUpdateUser(UpdateUserRawCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            var email = command.Email.ToLowerInvariant().Trim();
            using (var uow = uowFactory.Create())
            {
                var user = uow.UserRepository.FirstOrDefault(u => u.Id == command.UserId);
                if (user == null)
                {
                    throw new NotFoundException("User not found");
                }
                if (user.Email != email && uow.UserRepository.Any(x => x.Email == email))
                {
                    throw new DomainException("The user with the same email already exists");
                }

                user.Email = command.Email;
                user.UserName = command.Email;
                user.FirstName = command.FirstName;
                user.LastName = command.LastName;
                user.Country = command.Country;
                user.City = command.City;
                user.PasswordHash = command.PasswordHash;
                user.UpdatedAt = DateTime.Now;

                uow.SaveChanges();
                command.HasPassword = string.IsNullOrEmpty(user.PasswordHash) == false;
            }
        }
    }
}
