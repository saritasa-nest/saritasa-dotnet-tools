using System;
using System.ComponentModel.DataAnnotations;

namespace SandBox
{
    /// <summary>
    /// Product entity.
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        public string Name { get; set; }

        public DateTime? BestBefore { get; set; }

        public Product()
        {
        }

        public Product(int id, string name, DateTime? bestBefore = null)
        {
            Id = id;
            Name = name;
            BestBefore = bestBefore;
        }
    }
}
