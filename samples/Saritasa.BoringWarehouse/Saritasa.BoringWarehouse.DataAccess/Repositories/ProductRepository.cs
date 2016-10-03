using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Saritasa.BoringWarehouse.Domain.Products.Entities;
using Saritasa.BoringWarehouse.Domain.Products.Repositories;

namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    /// <summary>
    /// Product repository.
    /// </summary>
    public class ProductRepository : Tools.Ef.EfRepository<Product, AppDbContext>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override Product Get(object id, IEnumerable<Expression<Func<Product, object>>> includes = null)
        {
            return Find(p => p.Id == (int)id, includes).SingleOrDefault();
        }
    }
}
