using System;
using System.Data;
using ZergRushCo.Todosya.Domain;

namespace ZergRushCo.Todosya.DataAccess
{
    public class AppUnitOfWorkFactory : IAppUnitOfWorkFactory
    {
        public IAppUnitOfWork Create() => new AppUnitOfWork(new AppDbContext());

        public IAppUnitOfWork Create(IsolationLevel isolationLevel) => new AppUnitOfWork(new AppDbContext());
    }
}
