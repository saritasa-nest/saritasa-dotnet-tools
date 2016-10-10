namespace Saritasa.BoringWarehouse.Domain.Products.Queries
{
    using System.Collections.Generic;

    using Entities;

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
