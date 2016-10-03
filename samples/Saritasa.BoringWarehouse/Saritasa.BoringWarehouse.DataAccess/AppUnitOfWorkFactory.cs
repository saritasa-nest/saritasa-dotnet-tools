using System.Data;
using Saritasa.BoringWarehouse.Domain;

namespace Saritasa.BoringWarehouse.DataAccess
{
    /// <summary>
    /// Application unit of work factory.
    /// </summary>
    public class AppUnitOfWorkFactory : IAppUnitOfWorkFactory
    {
        public IAppUnitOfWork Create()
        {
            return new AppUnitOfWork(new AppDbContext());
        }

        public IAppUnitOfWork Create(IsolationLevel isolationLevel)
        {
            return new AppUnitOfWork(new AppDbContext());
        }
    }
}
