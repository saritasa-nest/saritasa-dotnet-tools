using Saritasa.Tools.Domain;

namespace Saritasa.BoringWarehouse.Domain
{
    /// <summary>
    /// Application unit of work factory.
    /// </summary>
    public interface IAppUnitOfWorkFactory : IUnitOfWorkFactory<IAppUnitOfWork>
    {
    }
}
