using System.Linq;
using Saritasa.Tools.Domain;
using Saritasa.BoringWarehouse.Domain.Products.Entities;
using Saritasa.BoringWarehouse.Domain.Products.Repositories;
using Saritasa.BoringWarehouse.Domain.Users.Entities;
using Saritasa.BoringWarehouse.Domain.Users.Repositories;

namespace Saritasa.BoringWarehouse.Domain
{
    /// <summary>
    /// Application unit of work. Logical wrapper of database context but with additional
    /// features.
    /// </summary>
    public interface IAppUnitOfWork : IUnitOfWork
    {
        IUserRepository UserRepository { get; }

        IQueryable<User> Users { get; }

        IProductRepository ProductRepository { get; }

        IQueryable<Product> Products { get; }

        ICompanyRepository CompanyRepository { get; }

        IQueryable<Company> Companies { get; }

        IProductPropertyRepository ProductPropertyRepository { get; }

        IQueryable<ProductProperty> ProductsProperties { get; }
    }
}
