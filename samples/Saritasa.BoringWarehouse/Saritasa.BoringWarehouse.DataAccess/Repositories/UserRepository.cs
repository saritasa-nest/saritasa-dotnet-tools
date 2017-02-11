namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    using Domain.Users.Entities;
    using Domain.Users.Repositories;

    /// <summary>
    /// Custom user repository.
    /// </summary>
    public class UserRepository : Tools.EF.EFRepository<User, AppDbContext>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }
    }
}
