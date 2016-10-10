namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Entities;

    public class UpdateProductCommand
    {
        public UpdateProductCommand()
        {
        }

        public UpdateProductCommand(Product product)
        {
            ProductId = product.Id;
            Name = product.Name;
            Quantity = product.Quantity;
            Sku = product.Sku;
            CompanyId = product.Company.Id;
            IsActive = product.IsActive;
            Properties = product.Properties == null ? new List<ProductProperty>() : new List<ProductProperty>(product.Properties);
            Comment = product.Comment;
        }

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

        public ICollection<ProductProperty> Properties { get; set; }

        [Required]
        [MaxLength(4096)]
        public string Comment { get; set; }

        public IEnumerable<Company> Companies { get; set; } = new List<Company>();

        [Required]
        public int UpdatedByUserId { get; set; }
    }
}
