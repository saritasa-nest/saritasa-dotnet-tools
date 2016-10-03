using System.Collections.Generic;
using Saritasa.BoringWarehouse.Domain.Products.Entities;

namespace Saritasa.BoringWarehouse.Domain.Products.Queries
{
    public class CompanyQueries
    {
        readonly IAppUnitOfWork uow;

        public CompanyQueries(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        public Company Get(int id)
        {
            return uow.CompanyRepository.Get(id);
        }

        public IEnumerable<Company> GetAll()
        {
            return uow.CompanyRepository.GetAll();
        }
    }
}
