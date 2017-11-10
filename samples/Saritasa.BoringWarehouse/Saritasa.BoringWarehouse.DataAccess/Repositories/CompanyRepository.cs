namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    using Domain.Products.Entities;
    using Domain.Products.Repositories;

    public class CompanyRepository : Saritasa.Tools.EF.EFRepository<Company, AppDbContext>, ICompanyRepository
    {
        public CompanyRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}
