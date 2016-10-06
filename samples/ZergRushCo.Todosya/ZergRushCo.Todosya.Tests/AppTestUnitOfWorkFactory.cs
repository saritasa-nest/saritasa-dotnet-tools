using System;
using System.Data;
using System.Data.Common;
using ZergRushCo.Todosya.DataAccess;
using ZergRushCo.Todosya.Domain;

namespace ZergRushCo.Todosya.Tests
{
    /// <summary>
    /// Unit of work factory for testing.
    /// </summary>
    public class AppTestUnitOfWorkFactory : IAppUnitOfWorkFactory
    {
        DbConnection connection;

        /// <summary>
        /// We need to have this variable to have persistent storage within test running.
        /// </summary>
        Guid id = Guid.NewGuid();

        public IAppUnitOfWork Create() => new AppUnitOfWork(CreateContext());

        public IAppUnitOfWork Create(IsolationLevel isolationLevel) => new AppUnitOfWork(CreateContext());

        public void SetSeedScenario1()
        {
            var dataLoader = new Effort.DataLoaders.CsvDataLoader(@"D:\work2\saritasatools\samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Tests\Seed1");
            connection = Effort.DbConnectionFactory.CreatePersistent(id.ToString(), dataLoader);
        }

        private AppDbContext CreateContext()
        {
            var context = connection != null ? new AppDbContext(connection) :
                new AppDbContext(Effort.DbConnectionFactory.CreatePersistent(id.ToString()));
            context.UseSqliteDatabase = false;
            return context;
        }
    }
}
