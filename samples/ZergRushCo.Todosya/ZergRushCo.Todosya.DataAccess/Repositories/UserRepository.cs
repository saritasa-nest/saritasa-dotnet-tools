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
        public UserRepository(AppDbContext context) : base(context)
        {
        }
    }
}
