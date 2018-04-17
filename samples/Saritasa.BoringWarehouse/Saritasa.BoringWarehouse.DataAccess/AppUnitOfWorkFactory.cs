using System.Data;
using Saritasa.BoringWarehouse.Domain;

namespace Saritasa.BoringWarehouse.DataAccess
{
    /// <summary>
    /// Application unit of work factory.
    /// </summary>
    public class AppUnitOfWorkFactory : IAppUnitOfWorkFactory
    {
        private readonly AppDbContext context;

        public AppUnitOfWorkFactory(AppDbContext context)
        {
            this.context = context;
        }

        public IAppUnitOfWork Create()
        {
            return new AppUnitOfWork(context);
        }

        public IAppUnitOfWork Create(IsolationLevel isolationLevel)
        {
            return new AppUnitOfWork(context);
        }
    }
}
