using System;
using Saritasa.Tools.Common.Pagination;

namespace SandBox
{
    /// <summary>
    /// Pagination samples.
    /// </summary>
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

            int offset = 0, limit = 10;
            var subset = OffsetLimitEnumerable.Create(products, offset, limit);

            var all = PagedEnumerable.Create(products, 2, 10);
            var all2 = all.CastMetadataEnumerable(p => new ProductWrapper(p));
            var dto = all2.ToMetadataObject();
        }
    }
}
