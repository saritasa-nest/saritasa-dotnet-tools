using System;
using System.ComponentModel.DataAnnotations;
using Saritasa.BoringWarehouse.Domain.Users.Entities;

namespace Saritasa.BoringWarehouse.Domain.Products.Entities
{
    /// <summary>
    /// Company that makes products.
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
