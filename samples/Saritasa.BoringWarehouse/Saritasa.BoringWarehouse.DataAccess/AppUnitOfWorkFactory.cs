namespace Saritasa.BoringWarehouse.DataAccess
{
    using System.Data;

    using Domain;

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
