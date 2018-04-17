using System.ComponentModel.DataAnnotations;

namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{
    /// <summary>
    /// Command to create new company.
    /// </summary>
    public class CreateCompanyCommand
    {
        [Key]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }
    }
}
