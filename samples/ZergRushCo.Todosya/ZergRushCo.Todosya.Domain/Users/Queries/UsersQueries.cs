using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
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

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="uow">Application unit of work.</param>
        public UsersQueries(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        /// <summary>
        /// .ctor for query pipeline.
        /// </summary>
        private UsersQueries()
        {
        }

        /// <summary>
        /// Get user by email address.
        /// </summary>
        /// <param name="email">Email address.</param>
        /// <returns>User instance or null if not found.</returns>
        public User GetByEmail(string email)
        {
            return uow.UserRepository.FirstOrDefault(u => u.Email == email);
        }

        /// <summary>
        /// Get user by id.
        /// </summary>
        /// <param name="id">User id.</param>
        /// <returns>User instance or null if not found.</returns>
        public User GetById(int id)
        {
            return uow.UserRepository.Get(id);
        }
    }
}
