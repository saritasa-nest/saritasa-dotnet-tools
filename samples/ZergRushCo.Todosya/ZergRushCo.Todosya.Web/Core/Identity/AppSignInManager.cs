using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Web.Core.Identity
{
    /// <summary>
    /// Application sign in manager implementation.
    /// </summary>
    public class AppSignInManager : SignInManager<User, int>
    {
        public AppSignInManager(UserManager<User, int> userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager)
        {
        }
    }
}
