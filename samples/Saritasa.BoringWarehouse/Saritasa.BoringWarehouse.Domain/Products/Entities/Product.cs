using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Saritasa.BoringWarehouse.Domain.Users.Entities;

namespace Saritasa.BoringWarehouse.Domain.Products.Entities
{
    /// <summary>
    /// Just simple product.
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public int Quantity { get; set; }

        public string Sku { get; set; }

        [Required]
        public Company Company { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<ProductProperty> Properties { get; set; } = new List<ProductProperty>();

        [Required]
        [MaxLength(4096)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Include Many to One and One to One relations.
        /// </summary>
        public static IEnumerable<Expression<Func<Product, object>>> DefaultInclude
        {
            get
            {
                yield return p => p.Company;
            }
        }

        /// <summary>
        /// Include all relations.
        /// </summary>
        public static IEnumerable<Expression<Func<Product, object>>> IncludeAll
        {
            get
            {
                yield return p => p.Company;
                yield return p => p.Properties;
            }
        }
    }
}
