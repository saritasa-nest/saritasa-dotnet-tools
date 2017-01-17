using System;
using System.Data;
using ZergRushCo.Todosya.Domain;

namespace ZergRushCo.Todosya.DataAccess
{
    /// <summary>
    /// Application unit of work factory.
    /// </summary>
    public class AppUnitOfWorkFactory : IAppUnitOfWorkFactory
    {
        private AppDbContext context;

        public AppUnitOfWorkFactory(AppDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Create unit of work instance.
        /// </summary>
        /// <returns>Application unit of work.</returns>
        public IAppUnitOfWork Create() => new AppUnitOfWork(context);

        /// <summary>
        /// Create unit of work with certain isolation level.
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level.</param>
        /// <returns>Application unit of work.</returns>
        public IAppUnitOfWork Create(IsolationLevel isolationLevel) => new AppUnitOfWork(context);
    }
}
