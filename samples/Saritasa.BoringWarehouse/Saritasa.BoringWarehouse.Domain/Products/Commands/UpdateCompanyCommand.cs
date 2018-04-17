using System.ComponentModel.DataAnnotations;
using Saritasa.BoringWarehouse.Domain.Products.Entities;

namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{
    /// <summary>
    /// Update company command.
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
