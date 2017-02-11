namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    using Tools.EF;

    using Domain.Products.Entities;
    using Domain.Products.Repositories;

    public class ProductPropertyRepository : EFRepository<ProductProperty, AppDbContext>, IProductPropertyRepository
    {
        public ProductPropertyRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}
