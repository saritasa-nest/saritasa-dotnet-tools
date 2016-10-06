using System;
using System.Linq;
using Saritasa.Tools.Commands;
using Saritasa.Tools.Events;
using Saritasa.Tools.Exceptions;
using ZergRushCo.Todosya.Domain.Users.Commands;
using ZergRushCo.Todosya.Domain.Users.Entities;
using ZergRushCo.Todosya.Domain.Users.Events;

namespace ZergRushCo.Todosya.Domain.Users.Handlers
{
    /// <summary>
    /// User handlers.
    /// </summary>
    [CommandHandlers]
    public class UserHandlers
    {
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
                uow.Complete();

                eventsPipeline.Raise(new UserCreatedEvent()
                {
                    User = user,
                });

                command.UserId = user.Id;
            }
        }

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

                uow.Complete();
                command.HasPassword = string.IsNullOrEmpty(user.PasswordHash) == false;
            }
        }

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

                uow.Complete();
                command.HasPassword = string.IsNullOrEmpty(user.PasswordHash) == false;
            }
        }
    }
}
