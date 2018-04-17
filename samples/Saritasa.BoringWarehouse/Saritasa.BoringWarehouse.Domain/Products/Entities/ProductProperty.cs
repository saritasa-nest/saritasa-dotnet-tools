using System.ComponentModel.DataAnnotations;

namespace Saritasa.BoringWarehouse.Domain.Products.Entities
{
    /// <summary>
    /// Product property.
    /// </summary>
    public class ProductProperty
    {
        [Key]
        public int Id { get; set; }

        public Product Product { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Value { get; set; }
    }
}
