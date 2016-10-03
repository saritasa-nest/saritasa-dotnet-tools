using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
