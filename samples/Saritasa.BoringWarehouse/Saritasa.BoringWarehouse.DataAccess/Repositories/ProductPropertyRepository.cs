namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    using Domain.Products.Entities;
    using Domain.Products.Repositories;
    using Tools.Ef;

    /// <summary>
    /// Product property repository.
    /// </summary>
    public class ProductPropertyRepository : EfRepository<ProductProperty, AppDbContext>, IProductPropertyRepository
    {
        public ProductPropertyRepository(AppDbContext context) 
            : base(context)
        {
        }
    }
}
