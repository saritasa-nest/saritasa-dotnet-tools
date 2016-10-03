using System.Collections.Generic;
using System.Linq;
using Saritasa.BoringWarehouse.Domain.Products.Entities;

namespace Saritasa.BoringWarehouse.Domain.Products.Queries
{
    public class ProductQueries
    {
        readonly IAppUnitOfWork uow;

        public ProductQueries(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        public Product Get(int id)
        {
            return uow.ProductRepository.Get(id, Product.IncludeAll);
        }

        public IEnumerable<Product> GetAll()
        {
            return uow.ProductRepository.GetAll(Product.DefaultInclude);
        }

        public PagedResult<Product> Search(ProductsObjectQuery objectQuery)
        {
            //
            return objectQuery.Search(GetAll());
        }
    }
}
