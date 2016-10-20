using System;
using System.Data;
using System.Data.Common;
using System.IO;
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

        /// <summary>
        /// Set seed scenario #1. The scenario contains 2 projects, 3 tasks and
        /// 1 user (email: test@saritasa.com, password: 111111).
        /// </summary>
        public void SetSeedScenario1()
        {
            var dataLoader = new Effort.DataLoaders.CsvDataLoader(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Seed1"));
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
