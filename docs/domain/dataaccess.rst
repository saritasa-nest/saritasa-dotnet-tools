Data Access
===========

Unit of Work
------------

Unit of Work design pattern does two important things: first it maintains in-memory updates and second it sends these in-memory updates as one transaction to the database.

To achieve this there are two interfaces available: ``IUnitOfWork`` and ``IUnitOfWorkFactory`` that placed to ``Saritasa.Tools.Domain`` namespace. The goal of ``IUnitOfWorkFactory`` is to create instance of ``IUnitOfWork``. Here is one of general use of the pattern:

1. Create custom ``AppDbContext`` inherited from ``DbContext``. Define your ``DbSet<T>`` members there.

2. Make your own ``IAppUnitOfWork`` and ``IAppUnitOfWork``. You will use it in your domain to abstract from data access infrastructure.

    Example:

        .. code-block:: c#

            public interface IAppUnitOfWork : IUnitOfWork
            {
                IUserRepository UserRepository { get; }
            }

            public interface IAppUnitOfWorkFactory : IUnitOfWorkFactory<IAppUnitOfWork> { }

    Please note that we use repository in context of unit of work.

3. Make implementation of ``IAppUnitOfWork`` and ``IAppUnitOfWork`` in your data access layer:

    .. code-block:: c#

        public class AppUnitOfWork : EFUnitOfWork<AppDbContext>, IAppUnitOfWork
        {
            public AppUnitOfWork(AppDbContext context) : base(context) { }

            public IUserRepository UserRepository => new UserRepository(Context);
        }

        public class AppUnitOfWorkFactory : IAppUnitOfWorkFactory
        {
            public IAppUnitOfWork Create() => new AppUnitOfWork(new AppDbContext());
            public IAppUnitOfWork Create(IsolationLevel isolationLevel) => new AppUnitOfWork(new AppDbContext());
        }

4. The things almost done, now you can access your data in abstract way:

    .. code-block:: c#

        using (var uow = unitOfWorkFactory.Create())
        {
            var email = command.Email.ToLowerInvariant().Trim();
            if (uow.UserRepository.Any(x => x.Email == email))
            {
                throw new DomainException("The user with the same email already exists");
            }

            var user = new User()
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                PasswordHash = command.Password,
            };
            uow.UserRepository.Add(user);
            uow.SaveChanges();
        }

5. For better usage also you can register them in you DI container. Here is an example for Autofac:

    .. code-block:: c#

        builder.RegisterType<DataAccess.AppDbContext>().AsSelf();
        builder.RegisterType<DataAccess.AppUnitOfWork>().AsImplementedInterfaces();
        builder.RegisterType<DataAccess.AppUnitOfWorkFactory>().AsImplementedInterfaces().SingleInstance();

Repository
----------

A Repository mediates between the domain and data mapping layers, acting like an in-memory domain object collection. In our implementation repository works in a context of unit of work. There are two interfaces available:

- ``IRepository``. Contains methods to get, find, add and remove entities from in-memory collection.
- ``IAsyncRepository``. Similar to ``IRepository`` but contains async methods.
- ``IQueryableRepository``. Provides the same methods as ``IRepository`` but also implements ``IQueryable`` interface. So you can query you data easily. Generally it is not recommended to return ``IQueryable`` from repository because it breaks pattern idea - the query is executed out of repository boundaries. But sometimes it may be convenient and useful.

.. note:: In ``Saritasa.Tools.EF6`` assembly you will find general implementations ``EFRepository`` and ``EFQueryableRepository``.

Repository Extensions
---------------------

.. function:: TEntity GetOrAdd<TEntity>(IRepository<TEntity> repository, params object[] keyValues)

    Get entity instance by id or create a new one.

.. function:: TEntity GetOrThrow<TEntity>(IRepository<TEntity> repository, params object[] keyValues)

    Get entity instance by id or generate ``NotFoundException`` exception.
