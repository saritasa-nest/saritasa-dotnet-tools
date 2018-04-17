using System.Linq;
using Saritasa.Tools.EF;
using Saritasa.BoringWarehouse.DataAccess.Repositories;
using Saritasa.BoringWarehouse.Domain;
using Saritasa.BoringWarehouse.Domain.Products.Entities;
using Saritasa.BoringWarehouse.Domain.Products.Repositories;
using Saritasa.BoringWarehouse.Domain.Users.Repositories;
using Saritasa.BoringWarehouse.Domain.Users.Entities;

namespace Saritasa.BoringWarehouse.DataAccess
{
    /// <summary>
    /// Application unit of work.
    /// </summary>
    public class AppUnitOfWork : EFUnitOfWork<AppDbContext>, IAppUnitOfWork
    {
        private readonly AppDbContext context;

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
