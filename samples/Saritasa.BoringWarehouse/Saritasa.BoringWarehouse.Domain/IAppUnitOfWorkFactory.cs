namespace Saritasa.BoringWarehouse.Domain
{
    using Tools.Domain;

    /// <summary>
    /// Application unit of work factory.
    /// </summary>
    public interface IAppUnitOfWorkFactory : IUnitOfWorkFactory<IAppUnitOfWork>
    {
    }
}
