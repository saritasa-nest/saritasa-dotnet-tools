//
// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
//

namespace Saritasa.Tools.Tests
{
    using System;
    using NUnit.Framework;
    using Saritasa.Tools.Domain;
    using System.Reflection;

    [TestFixture]
    public class CommandsTests
    {
        public class TestCommand : ICommand
        {
            public int Id { get; set; }
        }

        public class TestCommandHandler : ICommandHandler<TestCommand>
        {
            public void Handle(TestCommand command)
            {
            }
        }

        public class TestCommandWithResult : ICommand
        {
            public int Id { get; set; }
        }

        public class TestCommandWithResultHandler : ICommandHandler
        {
            public string Handle(TestCommandWithResult command)
            {
                return "test";
            }
        }

        [Test]
        public void Command_pipeline_test()
        {
            var cp = new CommandPipeline();
            cp.AddHandlers(
                new Domain.CommandPipelineMiddlewares.CommandHandlerLocatorMiddleware(Assembly.GetExecutingAssembly()),
                new Domain.CommandPipelineMiddlewares.CommandExecutorMiddleware(new Func<Type, object>((t) =>
                    {
                        return Activator.CreateInstance(t);
                    }))
            );

            cp.Execute(new TestCommand() { Id = 5 });
        }
    }
}
