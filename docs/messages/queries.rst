Queries
=======

Overview
--------

Query is a reading operation. Query always returns result and does not change system state. For example getting user by id, or getting list of products. To use queries with query pipeline you should pass method delegate and parameters. Here are steps required:

1. Setup pipeline service.

2. Prepare class with query methods, attribute ``QueryHandlers`` can be assigned (not required right now):

    .. code-block:: c#

        [QueryHandlers]
        public class TasksQueries
        {
            readonly IAppUnitOfWork uow;

            private TasksQueries()
            {
            }

            public TasksQueries(IAppUnitOfWork uow)
            {
                this.uow = uow;
            }

            public TaskDto GetByIdDto(int taskId)
            {
                return new TaskDto(uow.TaskRepository.Get(taskId));
            }
        }

    As you can see it is plain c# class. ``uow`` will be injected using your ``container.Resolve`` method. The reason of having private parameterless constructor is below.

3. Execute query method:
   
    .. code-block:: c#

        var task = ServicePipeline.Query<TasksQueries>().With(q => q.GetByIdDto(command.TaskId)); // #1
        // or
        var task = ServicePipeline.Query<TasksQueries>(tasksQueries).With(q => q.GetByIdDto(command.TaskId)); // #2
        // the same as
        var task = tasksQueries.GetByIdDto(3); // #3

    At first case ``Query<TasksQueries>()`` will create temporary object and ``TasksQueries`` will be resolved. So that ``TasksQueries`` must have parameterless constructor (public or private). At second case we presume that provided ``tasksQueries`` argument is already resolved by application. Third case is just usual method call and can be used when performance required.

.. note:: Query pipeline maked additional performance overhead because of reflection and additional calls. If performance makes sense inject query handler classes using your DI container and use them directly.

Middlewares
-----------

    .. class:: QueryExecutorMiddleware

        Executes query delegate. Included in default pipeline.

    .. class:: QueryObjectResolverMiddleware

        Resolve object handler for query. Included in default pipeline.

Default Pipeline
----------------

    ::

        QueryObjectResolverMiddleware ---> QueryExecutorMiddleware
