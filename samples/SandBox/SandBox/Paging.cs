using System;
using System.Linq;
using Saritasa.Tools.Common.Pagination;

namespace SandBox
{
    public static class Paging
    {
        private class ProductWrapper
        {
            public string Name { get; }

            public ProductWrapper(Product product)
            {
                this.Name = product.Name;
            }
        }

        public static void Try1()
        {
            var repository = new AnotherProductsRepository();
            var products = repository.GetAll();

            var all = PagedEnumerable.Create(products, 2, 10);
            var all2 = all.CastMetadataEnumerable(p => new ProductWrapper(p));
            var dto = all2.ToMetadataObject();
        }
    }
}
