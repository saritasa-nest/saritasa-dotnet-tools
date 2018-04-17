using Saritasa.BoringWarehouse.Domain.Users.Entities;

namespace Saritasa.BoringWarehouse.Domain.Users.Queries
{
    /// <summary>
    /// Various users queries.
    /// </summary>
    public class UserQueries
    {
        private readonly IAppUnitOfWork uow;

        public UserQueries(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        public User GetById(int id)
        {
            return uow.UserRepository.Get(id);
        }

        public PagedResult<User> Search(UsersObjectQuery objectQuery)
        {
            return objectQuery.Search(uow.Users);
        }
    }
}
