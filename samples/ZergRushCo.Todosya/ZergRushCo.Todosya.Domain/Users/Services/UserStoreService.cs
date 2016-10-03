using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Saritasa.Tools.Commands;
using ZergRushCo.Todosya.Domain.Users.Commands;
using ZergRushCo.Todosya.Domain.Users.Entities;
using ZergRushCo.Todosya.Domain.Users.Repositories;

namespace ZergRushCo.Todosya.Domain.Users.Services
{
    /// <summary>
    /// Our custom user store implementation that uses commands.
    /// </summary>
    public class UserStoreService :
        IUserPasswordStore<User, int>,
        IUserLockoutStore<User, int>,
        IUserTwoFactorStore<User, int>,
        IUserPhoneNumberStore<User, int>,
        IUserLoginStore<User, int>
    {
        readonly IUserRepository userRepository;
        readonly ICommandPipeline commandPipeline;

        public UserStoreService(IUserRepository userRepository, ICommandPipeline commandPipeline)
        {
            if (userRepository == null)
            {
                throw new ArgumentNullException(nameof(this.userRepository));
            }
            if (commandPipeline == null)
            {
                throw new ArgumentNullException(nameof(this.userRepository));
            }
            this.userRepository = userRepository;
            this.commandPipeline = commandPipeline;
        }

        #region IUserPasswordStore

        /// <inheritdoc />
        public Task CreateAsync(User user)
        {
            return Task.Run(() =>
            {
                commandPipeline.Handle(new RegisterUserCommand()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Password = user.PasswordHash,
                    Email = user.UserName,
                    Country = user.Country,
                    BirthDay = user.BirthDay,
                    City = user.City,
                });
            });
        }

        /// <inheritdoc />
        public Task DeleteAsync(User user)
        {
            return Task.Run(() => userRepository.Remove(user));
        }

        /// <inheritdoc />
        public Task<User> FindByIdAsync(int userId)
        {
            return Task.Run(() => userRepository.FirstOrDefault(u => u.Id == userId));
        }

        /// <inheritdoc />
        public Task<User> FindByNameAsync(string userName)
        {
            return Task.Run(() => userRepository.FirstOrDefault(u => u.UserName == userName));
        }

        /// <inheritdoc />
        public Task UpdateAsync(User user)
        {
            return Task.Run(() =>
            {
                commandPipeline.Handle(new UpdateUserRawCommand(user));
            });
        }

        /// <inheritdoc />
        public Task<string> GetPasswordHashAsync(User user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        /// <inheritdoc />
        public Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(true);
        }

        #endregion

        #region IUserLockoutStore

        /// <inheritdoc />
        public Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        {
            return Task.FromResult(user.LockoutEndDateUtc.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) : default(DateTimeOffset));
        }

        /// <inheritdoc />
        public Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        {
            user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? null : new DateTime?(lockoutEnd.UtcDateTime);
            return Task.FromResult(0);
        }

        /// <inheritdoc />
        public Task<int> IncrementAccessFailedCountAsync(User user)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ResetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<int> GetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <inheritdoc />
        public Task<bool> GetLockoutEnabledAsync(User user)
        {
            return Task.FromResult(user.IsLockoutEnabled);
        }

        /// <inheritdoc />
        public Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            user.IsLockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        #endregion

        #region IUserTwoFactorStore

        /// <inheritdoc />
        public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            user.IsTwoFactorEnabled = true;
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            return Task.FromResult(user.IsTwoFactorEnabled);
        }

        #endregion

        #region IUserPhoneNumberStore

        /// <inheritdoc />
        public Task SetPhoneNumberAsync(User user, string phoneNumber)
        {
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<string> GetPhoneNumberAsync(User user)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        /// <inheritdoc />
        public Task<bool> GetPhoneNumberConfirmedAsync(User user)
        {
            return Task.FromResult(user.IsPhoneNumberConfirmed);
        }

        /// <inheritdoc />
        public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed)
        {
            user.IsPhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        #endregion

        #region IUserLoginStore

        /// <inheritdoc />
        public Task AddLoginAsync(User user, UserLoginInfo login)
        {
            return Task.FromResult(0);
        }

        /// <inheritdoc />
        public Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            return Task.FromResult(0);
        }

        /// <inheritdoc />
        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            return Task.FromResult<IList<UserLoginInfo>>(new List<UserLoginInfo>());
        }

        /// <inheritdoc />
        public Task<User> FindAsync(UserLoginInfo login)
        {
            return Task.FromResult<User>(null);
        }

        #endregion

        public void Dispose()
        {
        }
    }
}
