namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    using Tools.Ef;

    using Domain.Products.Entities;
    using Domain.Products.Repositories;

    public class ProductPropertyRepository : EfRepository<ProductProperty, AppDbContext>, IProductPropertyRepository
    {
        public ProductPropertyRepository(AppDbContext context) 
            : base(context)
        {
        }
    }
}
