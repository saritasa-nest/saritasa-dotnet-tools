using System.Data.Common;
using System.Data.Entity;
using System.Data;
using Saritasa.Tools.Domain;
using Saritasa.Tools.EF;

namespace SandBox
{
    /// <summary>
    /// Example to quickly demo repository usage.
    /// </summary>
    public class CommonRepositoryUsageExample
    {
        /// <summary>
        /// 1. First create your application database context class with all database sets your need.
        /// It should be in data access layer of application.
        /// </summary>
        public class AppDbContext : DbContext
        {
            public AppDbContext()
            {
            }

            public AppDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
            {
            }

            public AppDbContext(DbConnection connection) : base(connection, true)
            {
            }

            public DbSet<Product> Products { get; set; }
        }

        /// <summary>
        /// 2. Products repository interface. It is lightweight and common. Should be in domain part of application.
        /// Also it can be extended with methods related to data access layer.
        /// </summary>
        public interface IProductRepository : IRepository<Product>
        {
            int GetSomethingFromStoredProcedure();
        }

        /// <summary>
        /// 3. User repository implementation. Should be in data access part of application.
        /// </summary>
        public class ProductRepository : Saritasa.Tools.EF.EFRepository<Product, AppDbContext>, IProductRepository
        {
            public ProductRepository(AppDbContext context) : base(context)
            {
            }

            public int GetSomethingFromStoredProcedure() => 42; // Raw sql, stored procedures, etc.
        }

        /// <summary>
        /// 4. Application unit of work that is in domain part of application.
        /// </summary>
        public interface IAppUnitOfWork : IUnitOfWork
        {
            IProductRepository ProductRepository { get; }
        }

        /// <summary>
        /// 5. Application unit of work implementation. Should be in data access part of application.
        /// </summary>
        public class AppUnitOfWork : Saritasa.Tools.EF.EFUnitOfWork<AppDbContext>, IAppUnitOfWork
        {
            public AppUnitOfWork(AppDbContext context) : base(context)
            {
            }

            public IProductRepository ProductRepository => new ProductRepository(Context);
        }

        /// <summary>
        /// 6. Application unit of work factory. Should be in domain part of application.
        /// </summary>
        public interface IAppUnitOfWorkFactory : IUnitOfWorkFactory<IAppUnitOfWork>
        {
        }

        /// <summary>
        /// 7. Implementation of application unit of work factory in data access part.
        /// </summary>
        public class AppUnitOfWorkFactory : EFUnitOfWorkFactory<AppDbContext>
        {
            public IAppUnitOfWork Create() => new AppUnitOfWork(new AppDbContext());

            public IAppUnitOfWork Create(IsolationLevel isolationLevel) => new AppUnitOfWork(new AppDbContext());
        }

        /// <summary>
        /// 8. Usage
        /// </summary>
        public void Usage()
        {
            // Register with DI container.
            var appUnitOfWorkFactory = new AppUnitOfWorkFactory();

            // In handler.
            using (var uow = appUnitOfWorkFactory.Create())
            {
                var product = uow.ProductRepository.Get(10);
                uow.ProductRepository.Add(new Product(10, "Test"));
                uow.SaveChanges();
            }
        }
    }

    /// <summary>
    /// More simple repository usage example.
    /// </summary>
    public class SimpleRepositoryUsageExample
    {
        /// <summary>
        /// 1. First create your application database context class with all database sets your need.
        /// It should be in data access layer of application.
        /// </summary>
        public class AppDbContext : DbContext
        {
            public AppDbContext()
            {
            }

            public AppDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
            {
            }

            public AppDbContext(DbConnection connection) : base(connection, true)
            {
            }

            public DbSet<Product> Products { get; set; }
        }

        /// <summary>
        /// 2. Application unit of work that is in domain part of application.
        /// </summary>
        public interface IAppUnitOfWork : IUnitOfWork
        {
            IQueryableRepository<Product> ProductRepository { get; }
        }

        /// <summary>
        /// 3. Application unit of work implementation. Should be in data access part of application.
        /// </summary>
        public class AppUnitOfWork : Saritasa.Tools.EF.EFUnitOfWork<AppDbContext>, IAppUnitOfWork
        {
            public AppUnitOfWork(AppDbContext context) : base(context)
            {
            }

            public IQueryableRepository<Product> ProductRepository =>
                new Saritasa.Tools.EF.EFQueryableRepository<Product, AppDbContext>(Context);
        }

        /// <summary>
        /// 4. Implementation of application unit of work factory in data access part.
        /// </summary>
        public class AppUnitOfWorkFactory : IUnitOfWorkFactory<IAppUnitOfWork>
        {
            public IAppUnitOfWork Create() => new AppUnitOfWork(new AppDbContext());

            public IAppUnitOfWork Create(IsolationLevel isolationLevel) => new AppUnitOfWork(new AppDbContext());
        }

        /// <summary>
        /// 5. Usage
        /// </summary>
        public void Usage()
        {
            // Register with DI container.
            var appUnitOfWorkFactory = new AppUnitOfWorkFactory();

            // In handler.
            using (var uow = appUnitOfWorkFactory.Create())
            {
                var product = uow.ProductRepository.Get(10);
                uow.ProductRepository.Add(new Product(10, "Test"));
            }
        }
    }
}
