using Microsoft.AspNet.Identity;
using ZergRushCo.Todosya.Domain.UserContext.Entities;

namespace ZergRushCo.Todosya.Domain.UserContext.Services
{
    /// <summary>
    /// Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity
    /// and is used by the application.
    /// </summary>
    public class AppUserManager : UserManager<User>
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="store">Application user story.</param>
        public AppUserManager(IUserStore<User> store) : base(store)
        {
            this.PasswordHasher = new AppPasswordHasher();
            // Use email as username.
            this.UserValidator = new UserValidator<User>(this)
            {
                AllowOnlyAlphanumericUserNames = false
            };
        }
    }
}
