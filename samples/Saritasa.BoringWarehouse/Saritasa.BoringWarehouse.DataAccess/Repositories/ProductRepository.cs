using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Saritasa.BoringWarehouse.Domain.Products.Entities;
using Saritasa.BoringWarehouse.Domain.Products.Repositories;

namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    /// <summary>
    /// Products repository.
    /// </summary>
    public class ProductRepository : Tools.EF.EFRepository<Product, AppDbContext>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public Product Get(int id, IEnumerable<Expression<Func<Product, object>>> includes = null)
        {
            return Find(p => p.Id == id, includes?.ToArray()).SingleOrDefault();
        }
    }
}
