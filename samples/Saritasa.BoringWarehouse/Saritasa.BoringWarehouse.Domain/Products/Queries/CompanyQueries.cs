using System.Collections.Generic;
using Saritasa.BoringWarehouse.Domain.Products.Entities;

namespace Saritasa.BoringWarehouse.Domain.Products.Queries
{
    /// <summary>
    /// Company related queries.
    /// </summary>
    public class CompanyQueries
    {
        private readonly IAppUnitOfWork uow;

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
