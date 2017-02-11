using Saritasa.Tools.Domain;
using Saritasa.Tools.EF;
using ZergRushCo.Todosya.DataAccess.Repositories;
using ZergRushCo.Todosya.Domain;
using ZergRushCo.Todosya.Domain.UserContext.Repositories;
using ZergRushCo.Todosya.Domain.TaskContext.Entities;

namespace ZergRushCo.Todosya.DataAccess
{
    /// <summary>
    /// Application unit of work.
    /// </summary>
    public class AppUnitOfWork : EFUnitOfWork<AppDbContext>, IAppUnitOfWork
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
        public IQueryableRepository<Project> ProjectRepository => new EFQueryableRepository<Project, AppDbContext>(Context);

        /// <summary>
        /// Tasks repository.
        /// </summary>
        public IQueryableRepository<Task> TaskRepository => new EFQueryableRepository<Task, AppDbContext>(Context);

        /// <summary>
        /// Users repository.
        /// </summary>
        public IUserRepository UserRepository => new UserRepository(Context);
    }
}
