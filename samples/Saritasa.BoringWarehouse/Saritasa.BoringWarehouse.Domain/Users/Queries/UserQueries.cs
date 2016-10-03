using Saritasa.BoringWarehouse.Domain.Users.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.BoringWarehouse.Domain.Users.Queries
{
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
