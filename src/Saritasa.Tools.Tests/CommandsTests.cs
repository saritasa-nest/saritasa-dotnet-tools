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

    [TestFixture]
    public class CommandsTests
    {
        [Command]
        public class TestCommand
        {
            public int Id { get; set; }

            public string Out { get; set; }
        }

        [CommandHandler]
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

            cp.Execute(new TestCommand() { Id = 5 });
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
        }

        [Test]
        public void Command_after_binary_seralizerdeserialize_should_be_equal()
        {
            var commandResult = new CommandExecutionResult((byte)0xEE);
            commandResult.CommandName = "test";
            commandResult.ExecutionDuration = 123;
            commandResult.CreatedAt = new DateTime(2015, 1, 1, 10, 10, 10);
            commandResult.Status = CommandExecutionContext.CommandStatus.Completed;

            using (var memory = new MemoryStream())
            {
                var binary = new Internal.CommandBinarySerializer(memory, new ByteObjectSerializer());
                binary.Write(commandResult);

                memory.Seek(0, SeekOrigin.Begin);
                var context2 = binary.Read();

                Assert.That(context2.CommandId, Is.EqualTo(commandResult.CommandId));
                Assert.That(context2.CreatedAt, Is.EqualTo(commandResult.CreatedAt));
                Assert.That(context2.ExecutionDuration, Is.EqualTo(commandResult.ExecutionDuration));
                Assert.That(context2.Status, Is.EqualTo(commandResult.Status));
                Assert.That(context2.Command, Is.EqualTo((byte)0xEE));
            }
        }
    }
}
