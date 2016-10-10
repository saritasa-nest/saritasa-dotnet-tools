namespace Saritasa.BoringWarehouse.Domain
{
    using System.Linq;

    using Tools.Domain;

    using Products.Entities;
    using Products.Repositories;
    using Users.Entities;
    using Users.Repositories;

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
