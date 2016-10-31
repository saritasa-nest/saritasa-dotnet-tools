// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using Saritasa.Tools.Messages.Events.PipelineMiddlewares;

namespace Saritasa.Tools.Tests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using NUnit.Framework;
    using Messages.Commands;
    using Messages.Commands.PipelineMiddlewares;

    [TestFixture]
    public class CommandsTests
    {
        #region Interfaces

        public interface IInterfaceA
        {
            string GetTestValue();
        }

        public class ImplementationA : IInterfaceA
        {
            public string GetTestValue() => "A";
        }

        public interface IInterfaceB
        {
            string GetTestValue();
        }

        public class ImplementationB : IInterfaceB
        {
            public string GetTestValue() => "B";
        }

        public static object InterfacesResolver(Type t)
        {
            if (t == typeof(IInterfaceA))
            {
                return new ImplementationA();
            }
            else if (t == typeof(IInterfaceB))
            {
                return new ImplementationB();
            }
            return null;
        }

        #endregion

        #region Can_run_default_simple_pipeline

        public class SimpleTestCommand
        {
            public int Id { get; set; }

            public string Out { get; set; }
        }

        [CommandHandlers]
        public class TestCommandHandlers1
        {
            public void HandleTestCommand(SimpleTestCommand command)
            {
                command.Out = "result";
            }
        }

        [Test]
        public void Can_run_default_simple_pipeline()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                Assembly.GetAssembly(typeof(CommandsTests))).UseInternalResolver(true);
            var cmd = new SimpleTestCommand() { Id = 5 };
            cp.Handle(cmd);
            Assert.That(cmd.Out, Is.EqualTo("result"));
        }

        #endregion

        #region Command_with_handle_in_it_should_run

        public class SimpleTestCommandWithHandler
        {
            public string Param { get; set; }

            public void Handle()
            {
                Param = "result";
            }
        }

        [Test]
        public void Command_with_handle_in_it_should_run()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                Assembly.GetAssembly(typeof(CommandsTests))).UseInternalResolver(true);
            var cmd = new SimpleTestCommandWithHandler();
            cp.Handle(cmd);
            Assert.That(cmd.Param, Is.EqualTo("result"));
        }

        #endregion

        #region Command_with_handle_in_it_and_deps_should_run

        public class TestCommandWithHandlerAndDeps
        {
            public string Param { get; set; }

            public void Handle(IInterfaceA a, IInterfaceB b)
            {
                Param = a.GetTestValue() + b.GetTestValue();
            }
        }

        [Test]
        public void Command_with_handle_in_it_and_deps_should_run()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(InterfacesResolver,
                Assembly.GetAssembly(typeof(CommandsTests))).UseInternalResolver(true);
            var cmd = new TestCommandWithHandlerAndDeps();
            cp.Handle(cmd);
            Assert.That(cmd.Param, Is.EqualTo("AB"));
        }

        #endregion

        #region Can_run_command_handler_with_public_properties_resolve

        public class TestCommand2
        {
            public int Param { get; set; }
        }

        [CommandHandlers]
        public class TestCommandHandlers2
        {
            public IInterfaceA DependencyA { get; set; }

            public void HandleTestCommand(TestCommand2 command)
            {
                command.Param = DependencyA.GetTestValue() == "A" ? 1 : 0;
            }
        }

        [Test]
        public void Can_run_command_handler_with_public_properties_resolve()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(InterfacesResolver,
                Assembly.GetAssembly(typeof(CommandsTests))).UseInternalResolver(true);
            var cmd = new TestCommand2();
            cp.Handle(cmd);
            Assert.That(cmd.Param, Is.EqualTo(1));
        }

        #endregion

        #region Can_run_command_handler_with_ctor_properties_resolve

        public class TestCommand3
        {
            public int Param { get; set; }
        }

        [CommandHandlers]
        public class TestCommandHandlers3
        {
            private readonly IInterfaceA dependencyA;

            public TestCommandHandlers3(IInterfaceA dependencyA)
            {
                this.dependencyA = dependencyA;
            }

            public void HandleTestCommand(TestCommand3 command)
            {
                command.Param = dependencyA.GetTestValue() == "A" ? 1 : 0;
            }
        }

        [Test]
        public void Can_run_command_handler_with_ctor_properties_resolve()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(InterfacesResolver,
                Assembly.GetAssembly(typeof(CommandsTests))).UseInternalResolver(true);
            var cmd = new TestCommand3();
            cp.Handle(cmd);
            Assert.That(cmd.Param, Is.EqualTo(1));
        }

        #endregion

        #region Validation_command_attributes_should_generate_exception

        class CommandWithValidation
        {
            [System.ComponentModel.DataAnnotations.Range(0, 100)]
            public int PercentInt { get; set; }

            [Required]
            public string Name { get; set; }

            public void Handle()
            {
                PercentInt = 10;
            }
        }

        [Test]
        public void Validation_command_attributes_should_generate_exception()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                Assembly.GetAssembly(typeof(CommandsTests))).UseInternalResolver(true);
            cp.InsertMiddlewareAfter(new CommandValidationMiddleware(), "CommandHandlerLocator");
            var cmd = new CommandWithValidation()
            {
                PercentInt = -10,
                Name = string.Empty,
            };

            bool exceptionFired = false;
            try
            {
                cp.Handle(cmd);
            }
            catch (CommandValidationException)
            {
                exceptionFired = true;
            }
            Assert.That(exceptionFired, Is.True);
            Assert.That(cmd.PercentInt, Is.Not.EqualTo(10));

            exceptionFired = false;
            cmd.PercentInt = 20;
            cmd.Name = "Mr Robot";
            try
            {
                cp.Handle(cmd);
            }
            catch (Exception)
            {
                exceptionFired = true;
            }
            Assert.That(exceptionFired, Is.False);
            Assert.That(cmd.PercentInt, Is.EqualTo(10));
        }

        #endregion

        #region If_command_handler_not_found_generate_exception

        class CommandWithNoHandler
        {
        }

        [Test]
        public void If_command_handler_not_found_generate_exception()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                Assembly.GetAssembly(typeof(CommandsTests)));

            bool exceptionFired = false;
            try
            {
                cp.Handle(new CommandWithNoHandler());
            }
            catch (CommandHandlerNotFoundException)
            {
                exceptionFired = true;
            }
            Assert.That(exceptionFired, Is.True);
        }

        #endregion
    }
}
