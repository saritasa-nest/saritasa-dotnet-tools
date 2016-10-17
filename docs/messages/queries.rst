#######
Queries
#######

********
Overview
********

Query is a reading operation. Query always returns result and does not change system state. For example getting user by id, or getting list of products. To use queries with query pipeline you should pass method delegate and parameters. Here are steps required:

1. Setup query pipeline for example with Autofac:
   
    .. code-block:: c#

            var builder = new ContainerBuilder();
            var container = builder.Build();
            var queryPipeline = Saritasa.Tools.Queries.QueryPipeline.CreateDefaultPipeline(container.Resolve);
            builder.RegisterInstance(queryPipeline).AsImplementedInterfaces().SingleInstance();

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

        var task = QueryPipeline.Execute(QueryPipeline.GetQuery<TasksQueries>().GetByIdDto, 3);
        // or
        var task = QueryPipeline.Execute(tasksQueries.GetByIdDto, 3);
        // the same as
        var task = tasksQueries.GetByIdDto(3);

    As you can see ``QueryPipeline.GetQuery`` is used to get method delegate. Maybe it looks ugly but it is only needed to get delegate type. To do that query handler must have default parameterless constructor either private or public.

.. note:: Query pipeline maked additional performance overhead because of reflection and additional calls. If performance makes sense inject query handler classes using your DI container and use them directly.

***********
Middlewares
***********

    .. class:: QueryExecutorMiddleware

        Executes query delegate. Included in default pipeline.

    .. class:: QueryObjectResolverMiddleware

        Resolve object handler for query. Included in default pipeline.
