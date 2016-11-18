// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using Xunit;
    using Messages.Common;
    using Messages.Commands;
    using Messages.Commands.PipelineMiddlewares;

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

        [Fact]
        public void Can_run_default_simple_pipeline()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                typeof(CommandsTests).GetTypeInfo().Assembly).UseInternalResolver(true);
            var cmd = new SimpleTestCommand() { Id = 5 };
            cp.Handle(cmd);
            Assert.Equal("result", cmd.Out);
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

        [Fact]
        public void Command_with_handle_in_it_should_run()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                typeof(CommandsTests).GetTypeInfo().Assembly).UseInternalResolver(true);
            var cmd = new SimpleTestCommandWithHandler();
            cp.Handle(cmd);
            Assert.Equal("result", cmd.Param);
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

        [Fact]
        public void Command_with_handle_in_it_and_deps_should_run()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(InterfacesResolver,
                typeof(CommandsTests).GetTypeInfo().Assembly).UseInternalResolver(true);
            var cmd = new TestCommandWithHandlerAndDeps();
            cp.Handle(cmd);
            Assert.Equal("AB", cmd.Param);
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

        [Fact]
        public void Can_run_command_handler_with_public_properties_resolve()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(InterfacesResolver,
                typeof(CommandsTests).GetTypeInfo().Assembly).UseInternalResolver(true);
            var cmd = new TestCommand2();
            cp.Handle(cmd);
            Assert.Equal(1, cmd.Param);
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

        [Fact]
        public void Can_run_command_handler_with_ctor_properties_resolve()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(InterfacesResolver,
                typeof(CommandsTests).GetTypeInfo().Assembly).UseInternalResolver(true);
            var cmd = new TestCommand3();
            cp.Handle(cmd);
            Assert.Equal(1, cmd.Param);
        }

        #endregion

        #region Validation_command_attributes_should_generate_exception

        class CommandWithValidation
        {
            [Range(0, 100)]
            public int PercentInt { get; set; }

            [Required]
            public string Name { get; set; }

            public void Handle()
            {
                PercentInt = 10;
            }
        }

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
        [Fact]
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

            Assert.Throws<CommandValidationException>(() => { cp.Handle(cmd); });
            Assert.NotEqual(10, cmd.PercentInt);

            cmd.PercentInt = 20;
            cmd.Name = "Mr Robot";
            cp.Handle(cmd);
            Assert.Equal(10, cmd.PercentInt);
        }
#endif

        #endregion

        #region If_command_handler_not_found_generate_exception

        class CommandWithNoHandler
        {
        }

        [Fact]
        public void If_command_handler_not_found_generate_exception()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                typeof(CommandsTests).GetTypeInfo().Assembly);

            Assert.Throws<CommandHandlerNotFoundException>(() => { cp.Handle(new CommandWithNoHandler()); });
        }

        #endregion

        #region Can find command handler by class name

        public class SimpleTestCommand2
        {
            public int Id { get; set; }

            public string Out { get; set; }
        }

        public class TestCommandHandlers
        {
            public void HandleTestCommand(SimpleTestCommand2 command)
            {
                command.Out = "out";
            }
        }

        [Fact]
        public void Can_find_handler_with_ClassSuffix_search_method()
        {
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                typeof(CommandsTests).GetTypeInfo().Assembly).UseInternalResolver(true).UseHandlerSearchMethod(HandlerSearchMethod.ClassSuffix);
            var cmd = new SimpleTestCommand2() { Id = 6 };
            cp.Handle(cmd);
            Assert.Equal("out", cmd.Out);
        }

        #endregion
    }
}
