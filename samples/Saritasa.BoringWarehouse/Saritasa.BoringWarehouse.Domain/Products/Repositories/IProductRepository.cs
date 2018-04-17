using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Saritasa.Tools.Domain;
using Saritasa.BoringWarehouse.Domain.Products.Entities;

namespace Saritasa.BoringWarehouse.Domain.Products.Repositories
{
    /// <summary>
    /// Product repository.
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        Product Get(int id, IEnumerable<Expression<Func<Product, object>>> includes = null);
    }
}
