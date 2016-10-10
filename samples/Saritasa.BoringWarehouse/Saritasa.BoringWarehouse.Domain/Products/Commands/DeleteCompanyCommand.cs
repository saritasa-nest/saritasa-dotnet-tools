namespace Saritasa.BoringWarehouse.Domain.Products.Commands
{
    using System.ComponentModel.DataAnnotations;

    using Entities;

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
