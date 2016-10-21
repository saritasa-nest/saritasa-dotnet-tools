using Saritasa.Tools.Ef;
using ZergRushCo.Todosya.Domain.Users.Entities;
using ZergRushCo.Todosya.Domain.Users.Repositories;

namespace ZergRushCo.Todosya.DataAccess.Repositories
{
    /// <summary>
    /// Users repository.
    /// </summary>
    public class UserRepository : EfQueryableRepository<User, AppDbContext>, IUserRepository
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
