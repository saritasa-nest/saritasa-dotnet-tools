using Saritasa.Tools.Domain;
using ZergRushCo.Todosya.Domain.Tasks.Entities;

namespace ZergRushCo.Todosya.Domain.Tasks.Repositories
{
    /// <summary>
    /// Interface for tasks repository.
    /// </summary>
    public interface ITaskRepository : IQueryableRepository<Task>
    {
    }
}
