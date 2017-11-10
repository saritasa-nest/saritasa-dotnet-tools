namespace Saritasa.BoringWarehouse.DataAccess.Repositories
{
    using System;

    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Domain.Products.Entities;
    using Domain.Products.Repositories;

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
