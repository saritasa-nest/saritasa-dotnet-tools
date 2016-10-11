using System;
using System.Collections.Generic;

namespace SandBox
{
    /// <summary>
    /// Simple repository interface.
    /// </summary>
    public interface IProductsRepository
    {
        /// <summary>
        /// Add product.
        /// </summary>
        /// <param name="product">Product.</param>
        void Add(Product product);

        /// <summary>
        /// Get product by id.
        /// </summary>
        /// <param name="id">Product id.</param>
        /// <returns>Product.</returns>
        Product Get(int id);

        /// <summary>
        /// Get all products.
        /// </summary>
        /// <returns>List of products.</returns>
        IEnumerable<Product> GetAll();
    }
}
