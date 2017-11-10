using System;
using System.ComponentModel.DataAnnotations;

namespace SandBox.Commands
{
    /// <summary>
    /// Test command about product update.
    /// </summary>
    public class UpdateProductCommand
    {
        public int ProductId { get; set; }

        [MaxLength(255)]
        [MinLength(2)]
        [Required]
        public string Name { get; set; }

        public DateTime? BestBefore { get; set; }
    }
}
