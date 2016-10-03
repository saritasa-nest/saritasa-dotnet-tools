using Saritasa.Tools.Domain;
using ZergRushCo.Todosya.Domain.Tasks.Entities;

namespace ZergRushCo.Todosya.Domain.Tasks.Repositories
{
    /// <summary>
    /// Interface for project repository.
    /// </summary>
    public interface IProjectRepository : IQueryableRepository<Project>
    {
    }
}
