using System.Linq;

namespace Saritasa.BoringWarehouse.Domain.Products.Queries
{
    using System.Collections.Generic;

    using Entities;

    public class ProductQueries
    {
        private readonly IAppUnitOfWork uow;

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
            return uow.ProductRepository.GetAll(Product.DefaultInclude.ToArray());
        }

        public PagedResult<Product> Search(ProductsObjectQuery objectQuery)
        {
            return objectQuery.Search(GetAll());
        }
    }
}
