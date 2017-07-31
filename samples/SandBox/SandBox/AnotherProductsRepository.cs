using System;
using System.Collections.Generic;
using System.Linq;

namespace SandBox
{
    public class AnotherProductsRepository : IProductsRepository
    {
        private readonly List<Product> list = new List<Product>
        {
            new Product(1, "The Intern 2015", new DateTime(2016, 10, 20)),
            new Product(2, "Gong fu yu jia"),
            new Product(3, "Intel Core i5"),
            new Product(4, "Cake"),
            new Product(5, "Apples"),
            new Product(6, "Soup"),
            new Product(7, "Neo"),
            new Product(8, "Philips"),
            new Product(9, "Earth"),
            new Product(10, "Moon"),
            new Product(11, "Radio"),
            new Product(12, "Angular"),
            new Product(13, "Trio"),
            new Product(14, "Life"),
            new Product(15, "Quake"),
            new Product(16, "Spinner"),
            new Product(17, ".NET"),
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
