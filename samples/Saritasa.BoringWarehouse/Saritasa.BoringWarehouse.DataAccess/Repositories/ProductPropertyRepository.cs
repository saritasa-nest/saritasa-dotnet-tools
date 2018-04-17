using Saritasa.Tools.EF;
using Saritasa.BoringWarehouse.Domain.Products.Entities;
using Saritasa.BoringWarehouse.Domain.Products.Repositories;

namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    /// <summary>
    /// Product property related repository.
    /// </summary>
    public class ProductPropertyRepository : EFRepository<ProductProperty, AppDbContext>, IProductPropertyRepository
    {
        public ProductPropertyRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}
