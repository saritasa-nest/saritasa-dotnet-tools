using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Users.Queries
{
    public class UsersQueries
    {
        readonly IAppUnitOfWork uow;

        public UsersQueries(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        public User GetById(int id)
        {
            return uow.UserRepository.Get(id);
        }
    }
}
