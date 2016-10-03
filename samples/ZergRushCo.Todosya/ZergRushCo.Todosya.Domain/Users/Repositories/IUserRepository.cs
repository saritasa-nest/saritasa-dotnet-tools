using Saritasa.Tools.Domain;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Users.Repositories
{
    /// <summary>
    /// Users repository.
    /// </summary>
    public interface IUserRepository : IQueryableRepository<User>
    {
    }
}
