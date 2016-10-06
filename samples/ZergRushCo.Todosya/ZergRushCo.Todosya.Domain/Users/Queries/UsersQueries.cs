using System.Linq;
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

        private UsersQueries()
        {
        }

        public User GetByEmail(string email)
        {
            return uow.UserRepository.FirstOrDefault(u => u.Email == email);
        }

        public User GetById(int id)
        {
            return uow.UserRepository.Get(id);
        }
    }
}
