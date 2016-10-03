using Saritasa.Tools.Domain;
using ZergRushCo.Todosya.Domain.Tasks.Entities;
using ZergRushCo.Todosya.Domain.Users.Repositories;

namespace ZergRushCo.Todosya.Domain
{
    /// <summary>
    /// Application interface for unit of work.
    /// </summary>
    public interface IAppUnitOfWork : IUnitOfWork
    {
        IUserRepository UserRepository { get; }

        IQueryableRepository<Project> ProjectRepository { get; }

        IQueryableRepository<Task> TaskRepository { get; }
    }
}
