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
        /// <summary>
        /// Users repository.
        /// </summary>
        IUserRepository UserRepository { get; }

        /// <summary>
        /// Projects repository.
        /// </summary>
        IQueryableRepository<Project> ProjectRepository { get; }

        /// <summary>
        /// Tasks repository.
        /// </summary>
        IQueryableRepository<Task> TaskRepository { get; }
    }
}
