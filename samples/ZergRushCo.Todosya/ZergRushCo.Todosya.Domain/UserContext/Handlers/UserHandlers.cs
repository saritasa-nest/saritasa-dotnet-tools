using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Domain.Exceptions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Abstractions.Events;
using ZergRushCo.Todosya.Domain.UserContext.Commands;
using ZergRushCo.Todosya.Domain.UserContext.Entities;
using ZergRushCo.Todosya.Domain.UserContext.Events;
using ZergRushCo.Todosya.Domain.UserContext.Exceptions;
using ZergRushCo.Todosya.Domain.UserContext.Services;

namespace ZergRushCo.Todosya.Domain.UserContext.Handlers
{
    /// <summary>
    /// User commands handlers.
    /// </summary>
    [CommandHandlers]
    public class UserHandlers
    {
        readonly ILogger logger;

        readonly IAppUnitOfWorkFactory uowFactory;

        readonly AppUserManager userManager;

        public UserHandlers(
            ILoggerFactory loggerFactory,
            IAppUnitOfWorkFactory uowFactory,
            AppUserManager userManager)
        {
            this.logger = loggerFactory.CreateLogger<UserHandlers>();
            this.uowFactory = uowFactory;
            this.userManager = userManager;
        }

        /// <summary>
        /// Handle user registration.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="pipelineService">Pipeline service.</param>
        public async Task HandleRegisterUser(
            RegisterUserCommand command,
            IMessagePipelineService pipelineService)
        {
            using (var uow = uowFactory.Create())
            {
                var email = command.Email.ToLowerInvariant().Trim();
                if (uow.UserRepository.Any(x => x.Email == email))
                {
                    throw new DomainException("The user with the same email already exists.");
                }

                var user = new User
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    City = command.City,
                    BirthDay = command.BirthDay,
                    Country = command.Country,
                    Email = command.Email,
                    UserName = email,
                };
                if (command.UserId != Guid.Empty)
                {
                    user.Id = command.UserId.ToString();
                }
                user.Clean();
                command.Result = await userManager.CreateAsync(user, command.Password);
                if (!command.Result.Succeeded)
                {
                    throw new IdentityException(command.Result);
                }

                await pipelineService.RaiseEventAsync(new UserCreatedEvent
                {
                    User = user,
                });

                command.User = user;
                logger.LogInformation($"User {user.FirstName} {user.LastName} with id {user.Id} has been registered.");
            }
        }

        /// <summary>
        /// Handle user update.
        /// </summary>
        /// <param name="command">Command.</param>
        public async Task HandleUpdateUser(UpdateUserCommand command)
        {
            var user = await userManager.FindByIdAsync(command.UserId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            user.Email = command.Email;
            user.UserName = command.Email;
            user.FirstName = command.FirstName;
            user.LastName = command.LastName;
            user.Country = command.Country;
            user.City = command.City;
            user.UpdatedAt = DateTime.Now;
            user.Clean();

            command.Result = await userManager.UpdateAsync(user);
            if (!command.Result.Succeeded)
            {
                throw new IdentityException(command.Result);
            }

            command.HasPassword = userManager.SupportsUserPassword;
        }

        public async Task HandleUpdateUserPassword(UpdateUserPasswordCommand command)
        {
            if (string.IsNullOrEmpty(command.CurrentPassword))
            {
                command.Result = await userManager.AddPasswordAsync(command.UserId, command.NewPassword);
            }
            else
            {
                command.Result = await userManager.ChangePasswordAsync(command.UserId, command.CurrentPassword, command.NewPassword);
            }

            if (!command.Result.Succeeded)
            {
                throw new IdentityException(command.Result);
            }
        }
    }
}
