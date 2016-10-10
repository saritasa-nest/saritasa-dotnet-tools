namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{
    using System.ComponentModel.DataAnnotations;

    using Entities;

    /// <summary>
    /// Update company command
    /// </summary>
    public class UpdateCompanyCommand
    {
        public UpdateCompanyCommand()
        {
        }

        public UpdateCompanyCommand(Company company)
        {
            CompanyId = company.Id;
            Name = company.Name;
        }

        [Key]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
    }
}
