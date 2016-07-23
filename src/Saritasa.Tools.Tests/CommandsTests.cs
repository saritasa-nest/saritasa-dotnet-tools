// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using System.Reflection;
    using NUnit.Framework;
    using Commands;
    using Commands.CommandPipelineMiddlewares;
    using System.IO;
    using Messages;

    [TestFixture]
    public class CommandsTests
    {
        [Command]
        public class TestCommand
        {
            public int Id { get; set; }

            public string Out { get; set; }
        }

        [CommandsHandler]
        public class TestCommandHandler
        {
            public void ExecuteTestCommand(TestCommand command)
            {
                command.Out = "out";
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
            cp.AddMiddlewares(
                new CommandHandlerLocatorMiddleware(Assembly.GetExecutingAssembly()),
                new CommandExecutorMiddleware(resolver)
            );

            cp.Handle(new TestCommand() { Id = 5 });
        }

        public class ByteObjectSerializer : IObjectSerializer
        {
            public object Deserialize(byte[] bytes, Type type)
            {
                return bytes[0];
            }

            public byte[] Serialize(object obj)
            {
                if (obj is byte)
                {
                    return new byte[] { 0xEE };
                }
                else
                {
                    throw new InvalidOperationException("Only byte is allowed");
                }
            }

            public bool IsText
            {
                get { return false; }
            }
        }
    }
}
