namespace Saritasa.BoringWarehouse.Domain.Products.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Tools.Domain;

    using Entities;

    public interface IProductRepository : IRepository<Product>
    {
        Product Get(int id, IEnumerable<Expression<Func<Product, object>>> includes = null);
    }
}
