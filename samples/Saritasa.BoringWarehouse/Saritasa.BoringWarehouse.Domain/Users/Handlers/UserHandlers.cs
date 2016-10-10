namespace Saritasa.BoringWarehouse.Domain.Users.Handlers
{
    using System;
    using System.Linq;

    using Tools.Commands;
    using Tools.Exceptions;

    using Commands;
    using Entities;

    /// <summary>
    /// User handlers.
    /// </summary>
    [CommandHandlers]
    public class UserHandlers
    {
        public void HandleCreate(CreateUserCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var email = command.Email.ToLowerInvariant().Trim();

                if (uow.Users.Any(x => x.Email == email))
                {
                    throw new DomainException("The user with the same email already exists");
                }

                var user = new User()
                {
                    Email = email,
                    PasswordHashed = Candy.SecurityUtils.Hash(command.Password, Candy.SecurityUtils.HashMethods.Sha256),
                    FirstName = command.FirstName.Trim(),
                    LastName = command.LastName.Trim(),
                    Phone = command.Phone,
                    Role = UserRole.Regular,
                };
                uow.UserRepository.Add(user);
                uow.Complete();
                command.UserId = user.Id;
            }
        }

        public void HandleLogin(LoginUserCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var email = command.Email.ToLowerInvariant().Trim();
                var user = uow.Users.FirstOrDefault(u => u.Email == email && u.IsActive);
                if (user == null)
                {
                    command.IsSuccess = false;
                    return;
                }

                var isPasswordCorrect = Candy.SecurityUtils.CheckHash(command.Password, user.PasswordHashed);
                if (!isPasswordCorrect)
                {
                    command.IsSuccess = false;
                    return;
                }

                command.User = user;
                command.IsSuccess = true;
            }
        }

        public void HandleUpdate(UpdateUserCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var email = command.Email.ToLowerInvariant().Trim();

                if (uow.Users.Any(x => x.Email == email && x.Id != command.UserId))
                {
                    throw new DomainException("The user with the same email already exists");
                }

                var dbUser = uow.UserRepository.Get(command.UserId);
                dbUser.Email = email;
                dbUser.FirstName = command.FirstName;
                dbUser.LastName = command.LastName;
                dbUser.Phone = command.Phone;
                dbUser.IsActive = command.IsActive;
                dbUser.UpdatedAt = DateTime.Now;

                if (string.IsNullOrEmpty(command.Password) == false)
                {
                    dbUser.PasswordHashed = Candy.SecurityUtils.Hash(command.Password, Candy.SecurityUtils.HashMethods.Sha256);
                }
                uow.Complete();
            }
        }
    }
}
