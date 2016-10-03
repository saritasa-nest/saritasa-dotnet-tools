using Saritasa.BoringWarehouse.Domain.Users.Entities;
using Saritasa.BoringWarehouse.Domain.Users.Repositories;

namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    /// <summary>
    /// User repository.
    /// </summary>
    public class UserRepository : Tools.Ef.EfRepository<User, AppDbContext>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }
    }
}
