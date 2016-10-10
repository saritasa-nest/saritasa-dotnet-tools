Commands
========

Command is something that changes the state (database insert/update/delete) of application. It utilizes Command behavioral design pattern: you should separate data for command and its handler. Here is a general usage:

1. Setup command pipeline, you can start with general one:

    .. code-block:: c#

        var commandPipeline = Saritasa.Tools.Commands.CommandPipeline.CreateDefaultPipeline(container.Resolve,
            System.Reflection.Assembly.GetAssembly(typeof(Domain.Users.Entities.User)));

    It will locate command handler and executes it. First argument is delegate to resolve dependencies. Second is set of assemblies where to locate handlers.

2. Create command, it should be POCO:

    .. code-block:: c#

        public class CreateProjectCommand
        {
            public string Name { get; set; }
            public string Color { get; set; } = "#2225AD";
            public int CreatedByUserId { get; set; }
            public int ProjectId { get; set; }
        }

3. Create command handler. To create it make new separate class, add ``CommandHandlers`` attribute to it and make a method within it where accept your command as first argument.

    .. code-block:: c#

        [CommandHandlers]
        public class ProjectHandlers
        {
            public void HandleCreateProject(CreateProjectCommand command, IAppUnitOfWorkFactory uowFactory)
            {
                // processing...
            }
        }

That's it!

**How to return data?**

In general you should not return any data from command. But in most cases you need at least the ``id`` of newly created entity. We recommend to make special property in command. For example command ``CreateProjectCommand`` has out field ``ProjectId``.

Rependencies Resolving
----------------------

There is how you can resolve dependencies for your command handlers:

- Using you handler class public constructor arguments.
- Using public properties of command handler.
- Using arguments of handler method.

Middlewares
-----------

    .. class:: CommandHandlerLocatorMiddleware

        Included to default pipeline. Locates for command handler class.

    .. class:: CommandExecutorMiddleware

        Included to default pipeline. Executes command against found command handler.

    .. class:: CommandValidationMiddleware

        Validates command against data annotation attributes. Generates ``CommandValidationException``.
