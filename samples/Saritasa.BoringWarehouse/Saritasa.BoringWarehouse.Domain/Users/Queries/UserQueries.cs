namespace Saritasa.BoringWarehouse.Domain.Users.Queries
{
    using Entities;

    /// <summary>
    /// Various users queries.
    /// </summary>
    public class UserQueries
    {
        IAppUnitOfWork uow;

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
