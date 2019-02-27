using System;
using System.Collections.Generic;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions.Queries;

namespace SandBox.Queries
{
    /// <summary>
    /// Products queries.
    /// </summary>
    [QueryHandlers]
    public class ProductsQueries
    {
        readonly IProductsRepository productsRepository;

        /// <summary>
        /// Need this ctor for query pipeline.
        /// </summary>
        private ProductsQueries()
        {
        }

        public ProductsQueries(IProductsRepository productsRepository)
        {
            this.productsRepository = productsRepository;
        }

        public IEnumerable<Product> Get(int offset, int limit)
        {
            Console.WriteLine("ProductsQueries: Get");
            return productsRepository.GetAll().Skip(offset).Take(limit).ToList();
        }
    }
}
