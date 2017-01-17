using Saritasa.Tools.Domain;
using ZergRushCo.Todosya.Domain.TaskContext.Entities;

namespace ZergRushCo.Todosya.Domain.TaskContext.Repositories
{
    /// <summary>
    /// Interface for tasks repository.
    /// </summary>
    public interface ITaskRepository : IQueryableRepository<Task>
    {
    }
}
