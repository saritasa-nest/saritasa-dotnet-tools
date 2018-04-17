using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Saritasa.BoringWarehouse.Domain.Products.Entities;

namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{
    /// <summary>
    /// Command to create new product.
    /// </summary>
    public class CreateProductCommand
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public int Quantity { get; set; }

        public string Sku { get; set; }

        [Required]
        public int CompanyId { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<ProductProperty> Properties { get; set; } = new List<ProductProperty>();

        [Required]
        [MaxLength(4096)]
        public string Comment { get; set; }

        public IEnumerable<Company> Companies { get; set; } = new List<Company>();

        [Required]
        public int CreatedByUserId { get; set; }
    }
}
