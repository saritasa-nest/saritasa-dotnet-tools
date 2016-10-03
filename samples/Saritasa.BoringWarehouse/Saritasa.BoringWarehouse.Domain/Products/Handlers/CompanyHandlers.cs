using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Commands;
using Saritasa.Tools.Exceptions;
using Saritasa.BoringWarehouse.Domain.Products.Commands;
using Saritasa.BoringWarehouse.Domain.Products.Entities;
using Saritasa.BoringWarehouse.Domain.Users.Entities;

namespace Saritasa.BoringWarehouse.Domain.Products.Handlers
{
    [CommandHandlers]
    public class CompanyHandlers
    {
        public void HandleCreate(CreateCompanyCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (IAppUnitOfWork uow = uowFactory.Create())
            {
                if (uow.Companies.Any(c => c.Name == command.Name.Trim()))
                {
                    throw new DomainException("The company with the same name already exists");
                }
                User creator = uow.Users.FirstOrDefault(u => u.Id == command.CreatedByUserId);
                if (creator == null)
                {
                    throw new DomainException("Can not find creator");
                }
                var company = new Company
                {
                    Name = command.Name,
                    CreatedBy = creator
                };
                uow.CompanyRepository.Add(company);
                uow.Complete();
                command.CompanyId = company.Id;
            }
        }
    }
}
