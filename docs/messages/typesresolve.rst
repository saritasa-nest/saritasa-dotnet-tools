#############
Types Resolve
#############

When you create default command, query or events pipeline you need to provide ``resolve`` function. It is used to resolve handlers therefor you should register all messages handlers with your IoC container. However you can use internal IoC container that is in Saritasa Tools library. By default we rely on your IoC setup. If you want to use internal IoC you should call ``UseInternalResolver`` extension method on your pipeline. Here is how it works:

- Using you handler class public constructor arguments.
- Using public properties of command handler.
- Using arguments of handler method (if specified).

For example following dependines will be resolved with internal IoC:

    .. code-block:: c#

        [CommandHandlers]
        public class TestCommandHandlers3
        {
            private readonly IInterfaceA dependencyA;

            public IInterfaceB DependencyB { get; set; }

            public TestCommandHandlers3(IInterfaceA dependencyA)
            {
                this.dependencyA = dependencyA;
            }

            public void HandleTestCommand(TestCommand3 command, IInterfaceC dependencyC)
            {
                command.Param = dependencyA.GetTestValue() == "A" ? 1 : 0;
            }
        }
