using Saritasa.Tools.Domain;
using Saritasa.Tools.Ef;
using ZergRushCo.Todosya.DataAccess.Repositories;
using ZergRushCo.Todosya.Domain;
using ZergRushCo.Todosya.Domain.Users.Repositories;
using ZergRushCo.Todosya.Domain.Tasks.Entities;

namespace ZergRushCo.Todosya.DataAccess
{
    public class AppUnitOfWork : EfUnitOfWork<AppDbContext>, IAppUnitOfWork
    {
        public AppUnitOfWork(AppDbContext context) : base(context)
        {
        }

        public IQueryableRepository<Project> ProjectRepository => new EfQueryableRepository<Project, AppDbContext>(Context);

        public IQueryableRepository<Task> TaskRepository => new EfQueryableRepository<Task, AppDbContext>(Context);

        public IUserRepository UserRepository => new UserRepository(Context);
    }
}
