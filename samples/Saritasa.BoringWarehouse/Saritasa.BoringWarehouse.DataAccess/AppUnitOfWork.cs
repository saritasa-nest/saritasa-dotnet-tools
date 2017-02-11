namespace Saritasa.BoringWarehouse.DataAccess
{
    using System.Linq;

    using Tools.EF;

    using Repositories;
    using Domain;
    using Domain.Products.Entities;
    using Domain.Products.Repositories;
    using Domain.Users.Repositories;
    using Domain.Users.Entities;

    /// <summary>
    /// Application unit of work.
    /// </summary>
    public class AppUnitOfWork : EFUnitOfWork<AppDbContext>, IAppUnitOfWork
    {
        AppDbContext context;

        public AppUnitOfWork(AppDbContext context)
            : base(context)
        {
            this.context = context;
        }

        public IUserRepository UserRepository => new UserRepository(Context);

        public IQueryable<User> Users => Context.Users;

        public IProductRepository ProductRepository => new ProductRepository(Context);

        public IQueryable<Product> Products => Context.Products;

        public ICompanyRepository CompanyRepository => new CompanyRepository(Context);

        public IQueryable<Company> Companies => Context.Companies;

        public IProductPropertyRepository ProductPropertyRepository => new ProductPropertyRepository(Context);

        public IQueryable<ProductProperty> ProductsProperties => Context.ProductProperties;
    }
}
