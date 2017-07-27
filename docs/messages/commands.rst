Commands
========

Overview
--------

Command is something that changes the state (database insert/update/delete) of application. It utilizes Command behavioral design pattern: you should separate data for command and its handler. Here is a general usage:

1. Setup pipeline service.

2. Create command, it should be POCO:

    .. code-block:: c#

        public class CreateProjectCommand
        {
            public string Name { get; set; }
            public string Color { get; set; } = "#2225AD";
            public int CreatedByUserId { get; set; }
            public int ProjectId { get; set; }
        }

3. Create command handler. To create it make new separate class, add ``CommandHandlers`` attribute to it and make a method within it where accept your command as first argument. Method name should start with ``Handle`` prefix and first argument should be command class.

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

Command contains data that needs for command execution - it is like model class in ASP.NET MVC. Try to implement command handler with all necessary dependencies it needs. Do not make "hidden" dependencies. This will make your code much clear and more testable. So think about the "black box" that has input and output.

    ::

        Input dependencies, services, command ---> [Box, actions] ---> Output (can be omitted)

**How to return data?**

In general you should not return any data from command. But in most cases you need at least the ``id`` of newly created entity. We recommend to make special property in command. For example command ``CreateProjectCommand`` has out field ``ProjectId``.

Middlewares
-----------

    .. class:: CommandHandlerLocatorMiddleware

        Included to default pipeline. Locates for command handler class using provided assemblies. Handler class must have ``CommandHandlers`` attribute, method should begin with ``Handle`` work and first argument must be command type.

    .. class:: CommandExecutorMiddleware

        Included to default pipeline. Executes command against found command handler.

    .. class:: CommandValidationMiddleware

        Validates command against data annotation attributes. Generates ``ValidationException``.

Default Pipeline
----------------

    ::

        CommandHandlerLocatorMiddleware ---> CommandExecutorMiddleware
