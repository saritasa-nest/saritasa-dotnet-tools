// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Commands;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// Command messages tests.
    /// </summary>
    public class CommandsTests
    {
        private readonly IMessagePipelineService pipelineService = new DefaultMessagePipelineService();

        #region Shared interfaces

        public interface Ns01_IInterfaceA
        {
            string GetTestValue();
        }

        public class Ns01_ImplementationA : Ns01_IInterfaceA
        {
            public string GetTestValue() => "A";
        }

        public interface Ns01_IInterfaceB
        {
            string GetTestValue();
        }

        public class Ns01_ImplementationB : Ns01_IInterfaceB
        {
            public string GetTestValue() => "B";
        }

        public static object Ns01_InterfacesResolver(Type t)
        {
            if (t == typeof(Ns01_IInterfaceA))
            {
                return new Ns01_ImplementationA();
            }
            if (t == typeof(Ns01_IInterfaceB))
            {
                return new Ns01_ImplementationB();
            }
            return null;
        }

        private void SetupCommandPipeline(CommandPipelineBuilder builder)
        {
            builder
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CommandsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerResolverMiddleware())
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });
        }

        #endregion

        #region Can_run_default_simple_pipeline

        public class Ns02_SimpleTestCommand
        {
            public int Id { get; set; }

            public string Out { get; set; }
        }

        [CommandHandlers]
        public class Ns02_TestCommandHandlers1
        {
            public void HandleTestCommand(Ns02_SimpleTestCommand command)
            {
                command.Out = "result";
            }
        }

        [Fact]
        public void Can_run_default_simple_pipeline()
        {
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline();
            SetupCommandPipeline(builder);
            var cmd = new Ns02_SimpleTestCommand { Id = 5 };

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.Equal("result", cmd.Out);
        }

        #endregion

        #region Can_run_default_simple_generic_command

        public class Ns03_SimpleTestGenericCommand<T>
        {
            public T Id { get; set; }

            public string Out { get; set; }
        }

        [CommandHandlers]
        public class Ns03_TestGenericCommandHandlers
        {
            public void HandleTestCommand<T>(Ns03_SimpleTestGenericCommand<T> command)
            {
                command.Out = "result";
            }
        }

        [Fact]
        public void Can_run_default_simple_generic_command()
        {
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline();
            SetupCommandPipeline(builder);
            var cmd = new Ns03_SimpleTestGenericCommand<string> { Id = "99" };

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.Equal("99", cmd.Id);
            Assert.Equal("result", cmd.Out);
        }

        #endregion

        #region Can_run_default_simple_generic_command_with_two_typed_params

        public class Ns12_SimpleTestGenericCommand<T1, T2>
        {
            public T1 Id { get; set; }

            public T2 Value { get; set; }

            public string Out { get; set; }
        }

        [CommandHandlers]
        public class Ns12_TestGenericCommandHandlers
        {
            public void HandleTestCommand<T1, T2>(Ns12_SimpleTestGenericCommand<T1, T2> command)
            {
                command.Out = "result";
            }
        }

        [Fact]
        public void Can_run_default_simple_generic_command_with_two_typed_params()
        {
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline();
            SetupCommandPipeline(builder);
            var cmd = new Ns12_SimpleTestGenericCommand<string, int> { Id = "99", Value = 99 };

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.Equal("99", cmd.Id);
            Assert.Equal(99, cmd.Value);
            Assert.Equal("result", cmd.Out);
        }

        #endregion

        #region Command_with_handle_in_it_should_run

        public class Ns04_SimpleTestCommandWithHandler
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
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline();
            SetupCommandPipeline(builder);
            var cmd = new Ns04_SimpleTestCommandWithHandler();

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.Equal("result", cmd.Param);
        }

        #endregion

        #region Command_with_handle_in_it_and_deps_should_run

        public class Ns05_TestCommandWithHandlerAndDeps
        {
            public string Param { get; set; }

            public void Handle(Ns01_IInterfaceA a, Ns01_IInterfaceB b)
            {
                Param = a.GetTestValue() + b.GetTestValue();
            }
        }

        [Fact]
        public void Command_with_handle_in_it_and_deps_should_run()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(Ns01_InterfacesResolver);
            var builder = pipelineService.PipelineContainer.AddCommandPipeline();
            SetupCommandPipeline(builder);
            var cmd = new Ns05_TestCommandWithHandlerAndDeps();

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.Equal("AB", cmd.Param);
        }

        #endregion

        #region Can_run_command_handler_with_public_properties_resolve

        public class Ns06_TestCommand
        {
            public int Param { get; set; }
        }

        [CommandHandlers]
        public class Ns06_TestCommandHandlers2
        {
            public Ns01_IInterfaceA DependencyA { get; set; }

            public Ns01_IInterfaceA DependencyB { get; }

            public Ns01_IInterfaceB DependencyC { get; set; } = new Ns01_ImplementationB();

            public void HandleTestCommand(Ns06_TestCommand command)
            {
                command.Param = DependencyA.GetTestValue() == "A"
                    && DependencyB == null
                    && DependencyC != null ? 1 : 0;
            }
        }

        [Fact]
        public void Can_run_command_handler_with_public_properties_resolve()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(Ns01_InterfacesResolver);
            var builder = pipelineService.PipelineContainer.AddCommandPipeline();
            builder
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CommandsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerResolverMiddleware
                {
                    UsePropertiesResolving = true
                })
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });
            var cmd = new Ns06_TestCommand();

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.Equal(1, cmd.Param);
        }

        #endregion

        #region Can_run_command_handler_with_ctor_resolve

        public class Ns07_TestCommand
        {
            public int Param { get; set; }
        }

        [CommandHandlers]
        public class Ns07_TestCommandHandlers3
        {
            private readonly Ns01_IInterfaceA dependencyA;

            public Ns07_TestCommandHandlers3(Ns01_IInterfaceA dependencyA)
            {
                this.dependencyA = dependencyA;
            }

            public void HandleTestCommand(Ns07_TestCommand command)
            {
                command.Param = dependencyA.GetTestValue() == "A" ? 1 : 0;
            }
        }

        [Fact]
        public void Can_run_command_handler_with_ctor_resolve()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(Ns01_InterfacesResolver);
            var builder = pipelineService.PipelineContainer.AddCommandPipeline();
            SetupCommandPipeline(builder);
            var cmd = new Ns07_TestCommand();

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.Equal(1, cmd.Param);
        }

        #endregion

        #region Validation_command_attributes_should_generate_exception

#if NET452
        private class Ns08_CommandWithValidation
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

        [Fact]
        public void Validation_command_attributes_should_generate_exception_for_bad_command()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(Ns01_InterfacesResolver);
            pipelineService.PipelineContainer.AddCommandPipeline()
            .AddMiddleware(new Commands.PipelineMiddlewares.CommandValidationMiddleware())
            .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                typeof(CommandsTests).GetTypeInfo().Assembly))
            .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware()
            {
                UseParametersResolve = true
            });
            var cmd = new Ns08_CommandWithValidation
            {
                PercentInt = -10,
                Name = string.Empty,
            };

            // Act & assert
            Assert.Throws<Domain.Exceptions.ValidationException>(() => { pipelineService.HandleCommand(cmd); });
            Assert.NotEqual(10, cmd.PercentInt);

            cmd.PercentInt = 20;
            cmd.Name = "Mr Robot";
            pipelineService.HandleCommand(cmd);
            Assert.Equal(10, cmd.PercentInt);
        }

        [Fact]
        public void Validation_command_attributes_should_not_generate_exception_for_good_command()
        {
            // Arrange
            pipelineService.PipelineContainer.AddCommandPipeline()
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandValidationMiddleware())
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CommandsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });
            var cmd = new Ns08_CommandWithValidation
            {
                PercentInt = 20,
                Name = "Mr Robot",
            };

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.Equal(10, cmd.PercentInt);
        }
#endif

        #endregion

        #region If_command_handler_not_found_generate_exception

        private class Ns09_CommandWithNoHandler
        {
        }

        [Fact]
        public void If_command_handler_not_found_generate_exception()
        {
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline();
            SetupCommandPipeline(builder);

            // Act & assert
            Assert.Throws<CommandHandlerNotFoundException>(() =>
            {
                pipelineService.HandleCommand(new Ns09_CommandWithNoHandler());
            });
        }

        #endregion

        #region Can_find_command_handler_by_class_name

        public class Ns10_SimpleTestCommand
        {
            public int Id { get; set; }

            public string Out { get; set; }
        }

        public class Ns10_TestCommandHandlers
        {
            public void HandleTestCommand(Ns10_SimpleTestCommand command)
            {
                command.Out = "out";
            }
        }

        [Fact]
        public void Can_find_handler_with_ClassSuffix_search_method()
        {
            // Arrange
            pipelineService.PipelineContainer.AddCommandPipeline()
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CommandsTests).GetTypeInfo().Assembly)
                {
                    HandlerSearchMethod = HandlerSearchMethod.ClassSuffix
                })
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerResolverMiddleware())
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });
            var cmd = new Ns10_SimpleTestCommand { Id = 6 };

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.Equal("out", cmd.Out);
        }

        #endregion

        #region Should_generate_in_pipeline_exception_on_fail

        private class Ns11_CommandWithFail
        {
            public void Handle(Ns11_CommandWithFail ns11Command)
            {
                throw new NotImplementedException("Test exception.");
            }
        }

        [Fact]
        public void Should_generate_in_pipeline_exception_on_fail()
        {
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline();
            SetupCommandPipeline(builder);

            // Act & assert
            Assert.Throws<MessageProcessingException>(() =>
            {
                pipelineService.HandleCommand(new Ns11_CommandWithFail());
            });
        }

        #endregion
    }
}
