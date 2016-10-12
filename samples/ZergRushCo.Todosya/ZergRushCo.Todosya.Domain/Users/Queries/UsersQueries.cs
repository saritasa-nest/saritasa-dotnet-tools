using System.Linq;
using Saritasa.Tools.Queries;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Users.Queries
{
    /// <summary>
    /// Useres queries.
    /// </summary>
    [QueryHandlers]
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
