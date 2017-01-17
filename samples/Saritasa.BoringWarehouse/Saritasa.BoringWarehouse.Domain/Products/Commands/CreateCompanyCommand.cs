namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{
    using System.ComponentModel.DataAnnotations;

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
