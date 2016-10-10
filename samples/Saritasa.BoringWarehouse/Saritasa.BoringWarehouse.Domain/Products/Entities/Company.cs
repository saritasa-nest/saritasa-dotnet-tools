namespace Saritasa.BoringWarehouse.Domain.Products.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using Users.Entities;

    /// <summary>
    /// Company that made product.
    /// </summary>
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public User CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
