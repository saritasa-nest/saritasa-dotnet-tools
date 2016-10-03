using Saritasa.Tools.Domain;

namespace ZergRushCo.Todosya.Domain
{
    /// <summary>
    /// Application interface for unit of work factory.
    /// </summary>
    public interface IAppUnitOfWorkFactory : IUnitOfWorkFactory<IAppUnitOfWork>
    {
    }
}
