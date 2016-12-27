using Saritasa.Tools.Domain;
using ZergRushCo.Todosya.Domain.TaskContext.Entities;

namespace ZergRushCo.Todosya.Domain.TaskContext.Repositories
{
    /// <summary>
    /// Interface for project repository.
    /// </summary>
    public interface IProjectRepository : IQueryableRepository<Project>
    {
    }
}
