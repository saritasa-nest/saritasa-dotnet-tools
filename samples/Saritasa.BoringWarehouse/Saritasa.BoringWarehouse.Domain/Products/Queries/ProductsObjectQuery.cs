namespace Saritasa.BoringWarehouse.Domain.Products.Queries
{
    using System.Collections.Generic;
    using System.Linq;

    using Tools.Common.Extensions;

    using Entities;

    public class ProductsObjectQuery : BaseObjectQuery
    {
        public PagedResult<Product> Search(IEnumerable<Product> products)
        {
            // Get total record count
            long total = products.LongCount();
            // Filtering
            if (!string.IsNullOrEmpty(SearchPattern))
            {
                // Find by name
                products = products.Where(p => p.Name.StartsWith(SearchPattern));
            }
            // Get count after filtering
            long filteredCount = products.LongCount();
            // Order
            switch (OrderColumn?.ToLower())
            {
                case "name":
                    products = products.Order(p => p.Name, SortOrder);
                    break;
                case "quantity":
                    products = products.Order(p => p.Quantity, SortOrder);
                    break;
                case "sku":
                    products = products.Order(p => p.Sku, SortOrder);
                    break;
                default:
                    products = products.Order(p => p.Id, SortOrder);
                    break;
            }
            products = products.Skip(Offset).Take(Limit);
            return new PagedResult<Product>(products.ToList(), Offset, Limit, total, filteredCount);
        }
    }
}
