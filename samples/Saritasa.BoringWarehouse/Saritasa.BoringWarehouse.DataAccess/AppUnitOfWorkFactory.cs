namespace Saritasa.BoringWarehouse.DataAccess
{
    using System.Data;

    using Domain;

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
