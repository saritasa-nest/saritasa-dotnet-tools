// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using NUnit.Framework;
    using Saritasa.Tools.Domain;
    using System.Reflection;

    [TestFixture]
    public class CommandsTests
    {
        [Command]
        public class TestCommand
        {
            public int Id { get; set; }
        }

        [CommandHandler]
        public class TestCommandHandler
        {
            public void HandleTestCommand(TestCommand command)
            {
            }
        }

        [Test]
        public void Command_pipeline_test()
        {
            var cp = new CommandPipeline();
            var resolver = new Func<Type, object>((t) =>
            {
                return Activator.CreateInstance(t);
            });
            cp.AddHandlers(
                new Domain.CommandPipelineMiddlewares.CommandHandlerLocatorMiddleware(Assembly.GetExecutingAssembly()),
                new Domain.CommandPipelineMiddlewares.CommandExecutorMiddleware(resolver)
            );

            cp.Execute(new TestCommand() { Id = 5 });
        }
    }
}
