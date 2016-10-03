using Saritasa.BoringWarehouse.Domain.Products.Entities;
using Saritasa.BoringWarehouse.Domain.Products.Repositories;

namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    /// <summary>
    /// Company repository.
    /// </summary>
    public class CompanyRepository : Tools.Ef.EfRepository<Company, AppDbContext>, ICompanyRepository
    {
        public CompanyRepository(AppDbContext context) 
            : base(context)
        {
        }
    }
}
