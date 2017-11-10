using System;
using System.Collections.Generic;
using System.Linq;

namespace SandBox
{
    /// <inheritdoc />
    public class ProductsRepository : IProductsRepository
    {
        private readonly List<Product> list = new List<Product>
        {
            new Product(1, "Milk", new DateTime(2016, 10, 20)),
            new Product(2, "GeForce GTX970"),
            new Product(3, "ChocoPie", new DateTime(2029, 1, 1)),
            new Product(4, "Cake"),
        };

        /// <inheritdoc />
        public void Add(Product product)
        {
            list.Add(product);
        }

        /// <inheritdoc />
        public Product Get(int id)
        {
            return list.FirstOrDefault(l => l.Id == id);
        }

        /// <inheritdoc />
        public IEnumerable<Product> GetAll()
        {
            return list;
        }
    }
}
