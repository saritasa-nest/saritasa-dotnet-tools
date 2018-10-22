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
            var subset = OffsetLimitListFactory.FromSource(products, offset, limit);

            var all = PagedListFactory.FromSource(products, 2, 10);
            var all2 = all.Convert(p => new ProductWrapper(p));
            var dto = all2.ToMetadataObject();
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
        }
    }
}
