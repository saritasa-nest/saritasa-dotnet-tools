using System.ComponentModel.DataAnnotations;
using Saritasa.BoringWarehouse.Domain.Products.Entities;

namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{

    /// <summary>
    /// Command to delete company by id.
    /// </summary>
    public class DeleteCompanyCommand
    {
        public DeleteCompanyCommand()
        {
        }

        public DeleteCompanyCommand(Company company)
        {
            CompanyId = company.Id;
        }

        [Key]
        public int CompanyId { get; set; }
    }
}
