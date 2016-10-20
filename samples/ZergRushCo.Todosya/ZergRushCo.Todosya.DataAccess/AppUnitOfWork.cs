using Saritasa.Tools.Domain;
using Saritasa.Tools.Ef;
using ZergRushCo.Todosya.DataAccess.Repositories;
using ZergRushCo.Todosya.Domain;
using ZergRushCo.Todosya.Domain.Users.Repositories;
using ZergRushCo.Todosya.Domain.Tasks.Entities;

namespace ZergRushCo.Todosya.DataAccess
{
    /// <summary>
    /// Application unit of work.
    /// </summary>
    public class AppUnitOfWork : EfUnitOfWork<AppDbContext>, IAppUnitOfWork
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="context">Application db context.</param>
        public AppUnitOfWork(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Projects repository.
        /// </summary>
        public IQueryableRepository<Project> ProjectRepository => new EfQueryableRepository<Project, AppDbContext>(Context);

        /// <summary>
        /// Tasks repository.
        /// </summary>
        public IQueryableRepository<Task> TaskRepository => new EfQueryableRepository<Task, AppDbContext>(Context);

        /// <summary>
        /// Users repository.
        /// </summary>
        public IUserRepository UserRepository => new UserRepository(Context);
    }
}
