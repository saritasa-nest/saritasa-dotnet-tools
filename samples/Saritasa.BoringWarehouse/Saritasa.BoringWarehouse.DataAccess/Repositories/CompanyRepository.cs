namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    using Domain.Products.Entities;
    using Domain.Products.Repositories;

    public class CompanyRepository : Saritasa.Tools.Ef.EfRepository<Company, AppDbContext>, ICompanyRepository
    {
        public CompanyRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}
