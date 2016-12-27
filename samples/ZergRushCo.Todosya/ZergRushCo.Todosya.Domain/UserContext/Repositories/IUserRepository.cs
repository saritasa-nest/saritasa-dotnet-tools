using Saritasa.Tools.Domain;
using ZergRushCo.Todosya.Domain.UserContext.Entities;

namespace ZergRushCo.Todosya.Domain.UserContext.Repositories
{
    /// <summary>
    /// Users repository.
    /// </summary>
    public interface IUserRepository : IQueryableRepository<User>
    {
    }
}
