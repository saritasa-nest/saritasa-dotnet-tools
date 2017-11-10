using Saritasa.Tools.EF;
using ZergRushCo.Todosya.Domain.UserContext.Entities;
using ZergRushCo.Todosya.Domain.UserContext.Repositories;

namespace ZergRushCo.Todosya.DataAccess.Repositories
{
    /// <summary>
    /// Users repository.
    /// </summary>
    public class UserRepository : EFQueryableRepository<User, AppDbContext>, IUserRepository
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="context">Application database context.</param>
        public UserRepository(AppDbContext context) : base(context)
        {
        }
    }
}
