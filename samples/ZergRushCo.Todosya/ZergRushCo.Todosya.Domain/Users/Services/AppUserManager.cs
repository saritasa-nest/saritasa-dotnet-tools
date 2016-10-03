using Microsoft.AspNet.Identity;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Users.Services
{
    /// <summary>
    /// Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity
    /// and is used by the application.
    /// </summary>
    public class AppUserManager : UserManager<User, int>
    {
        public AppUserManager(IUserStore<User, int> store) : base(store)
        {
            this.PasswordHasher = new AppPasswordHasher();
        }
    }
}
