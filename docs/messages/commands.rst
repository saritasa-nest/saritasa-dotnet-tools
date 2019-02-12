Commands
========

Overview
--------

Command is something that changes the state (database insert/update/delete) of application. It uses Command behavioral design pattern: you should separate data to command and its handler. Here is a general usage:

1. Setup pipeline service. Here is a code for pipeline you can use for most projects:

    .. code-block:: c#

            // Setup.
            var pipeline = pipelineService.PipelineContainer.AddCommandPipeline();
            pipeline.AddMiddleware(new Saritasa.Tools.Messages.Common.PipelineMiddlewares.PrepareMessageContextMiddleware());
            pipeline.AddMiddleware(new Saritasa.Tools.Messages.Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware());
            pipeline.AddMiddleware(new Saritasa.Tools.Messages.Commands.PipelineMiddlewares.CommandHandlerResolverMiddleware());
            pipeline.AddMiddleware(new Saritasa.Tools.Messages.Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware()
            {
                CaptureExceptionDispatchInfo = true
            });

    There is a simpler way to do that:

    .. code-block:: c#

        // Setup.
        var pipelineService = new DefaultMessagePipelineService();
        pipelineService.PipelineContainer
            .AddCommandPipeline()
                .AddStandardMiddlewares();

    The ``AddStandardMiddlewares`` also accepts optional options argument.

2. Create command, it should be POCO:

    .. code-block:: c#

        public class CreateProjectCommand
        {
            public string Name { get; set; }
            public string Color { get; set; } = "#2225AD";
            public int CreatedByUserId { get; set; }
            public int ProjectId { get; set; }
        }

3. Create command handler. Make new separate class, add ``CommandHandlers`` attribute to it and add new method that accepts your command as first argument. Method name should start with ``Handle`` prefix and first argument should be command class.

    .. code-block:: c#

        [CommandHandlers]
        public class ProjectHandlers
        {
            public void HandleCreateProject(CreateProjectCommand command, IAppUnitOfWorkFactory uowFactory)
            {
                // Processing...
            }
        }

4. Execute command using pipeline service:

    .. code-block:: c#

        CreateProjectCommand command = new CreateProjectCommand() { Name = "Test", CreatedByUserId = CurrentUser.Id };
        ServicePipeline.HandleCommand(command);

That's it!

Command contains data that is required for command execution - it is like model class in ASP.NET MVC. Try to implement command handler with all necessary dependencies it needs. Do not make "hidden" dependencies. This will make your code much clear and more testable. So think about the "black box" that has input and output.

    ::

        Input dependencies, services, command ---> [Box, actions] ---> Output (can be omitted)

**How to return data?**

In general you should not return any data from command. But in most cases you need at least the ``id`` of newly created entity. We recommend to make special property in command. For example command ``CreateProjectCommand`` has out field ``ProjectId``.

Middlewares
-----------

    .. class:: PrepareMessageContextMiddleware

        The middleware prepares message context for message processing. It fills ContentId field.

    .. class:: CommandHandlerLocatorMiddleware

        Included to default pipeline. Locates for command handler class using provided assemblies. Handler class must have ``CommandHandlers`` attribute, method should begin with ``Handle`` work and first argument must be command type. The resolved method is stored in ``handler-method`` item of context items.

    .. class:: CommandHandlerResolverMiddleware

        The middleware is to resolve handler object, create it and resolve all dependencies if needed. The resolved object is stored in ``handler-object`` item of context items.

        .. attribute:: UsePropertiesResolving

            Resolve handler object public properties using service provider. False by default.

    .. class:: CommandHandlerExecutorMiddleware

        Executes command against found command handler.

        .. attribute:: IncludeExecutionDuration

            Includes execution duration into processing result. The target item key is ``.execution-duration``. Default is true.

        .. attribute:: UseParametersResolve

            If true the middleware will try to resolve executing method parameters. False by default.

        .. attribute:: CaptureExceptionDispatchInfo

            Captures original exception and stack trace within handler method using ``System.Runtime.ExceptionServices.ExceptionDispatchInfo``. False by default.

Default Pipeline
----------------

    ::

        PrepareMessageContextMiddleware ---> CommandHandlerLocatorMiddleware ---> CommandHandlerResolverMiddleware ---> CommandHandlerExecutorMiddleware
