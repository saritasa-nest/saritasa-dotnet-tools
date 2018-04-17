using Saritasa.BoringWarehouse.Domain.Products.Entities;
using Saritasa.BoringWarehouse.Domain.Products.Repositories;

namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    /// <summary>
    /// Company repository.
    /// </summary>
    public class CompanyRepository : Saritasa.Tools.EF.EFRepository<Company, AppDbContext>, ICompanyRepository
    {
        public CompanyRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}
