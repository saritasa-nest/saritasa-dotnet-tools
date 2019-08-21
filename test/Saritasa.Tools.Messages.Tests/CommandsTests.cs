// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
    public sealed class CommandsTests
    {
        private readonly IMessagePipelineService pipelineService = new DefaultMessagePipelineService();

        #region Shared interfaces

#pragma warning disable SA1302 // Interface names should begin with I
        public interface Ns01_IInterfaceA
#pragma warning restore SA1302 // Interface names should begin with I
        {
            string GetTestValue();
        }

        public class Ns01_ImplementationA : Ns01_IInterfaceA
        {
            public string GetTestValue() => "A";
        }

#pragma warning disable SA1302 // Interface names should begin with I
        public interface Ns01_IInterfaceB
#pragma warning restore SA1302 // Interface names should begin with I
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
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware());
        }

        #endregion

        #region HandleCommand_CommandIsNull_ShouldRaiseInvalidOperationException

        [Fact]
        public void HandleCommand_CommandIsNull_ShouldRaiseInvalidOperationException()
        {
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline()
                .AddStandardMiddlewares(options =>
                {
                    options.Assemblies =
                        new[] { typeof(CommandsTests).GetTypeInfo().Assembly };
                });

            // Act & assert
            Assert.Throws<InvalidOperationException>(() => { pipelineService.HandleCommand(null); });
        }

        #endregion

        #region HandleCommand_DefaultSimplePipeline_ResultReturned

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
        public void HandleCommand_DefaultSimplePipeline_ResultReturned()
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

        #region HandleCommand_GenericInputCommandWithOneTypedParam_ResultReturned

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
        public void HandleCommand_GenericInputCommandWithOneTypedParam_ResultReturned()
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

        #region HandleCommand_DefaultSimpleGenericCommandWithTwoTypedParams_ResultReturned

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
        public void HandleCommand_DefaultSimpleGenericCommandWithTwoTypedParams_ResultReturned()
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

        #region HandleCommand_CommandWithHandleMethonInIt_ResultReturned

        public class Ns04_SimpleTestCommandWithHandler
        {
            public string Param { get; set; }

            public void Handle()
            {
                Param = "result";
            }
        }

        [Fact]
        public void HandleCommand_CommandWithHandleMethonInIt_ResultReturned()
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

        #region HandleCommand_CommandWithHandleMethonInItAndDependencies_ResultReturned

        public class Ns05_TestCommandWithHandlerAndDeps
        {
            public string Param { get; set; }

            public void Handle(Ns01_IInterfaceA a, Ns01_IInterfaceB b)
            {
                Param = a.GetTestValue() + b.GetTestValue();
            }
        }

        [Fact]
        public void HandleCommand_CommandWithHandleMethonInItAndDependencies_ResultReturned()
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

        #region HandleCommand_CommandHandlerHasPublicPropertiesToResolve_PropertiesResolvedCorrectly

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
                    && DependencyC != null ? 1 : 0; // 1 means resolved properly
            }
        }

        [Fact]
        public void HandleCommand_CommandHandlerHasPublicPropertiesToResolve_PropertiesResolvedCorrectly()
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

        #region HandleCommand_CommandHandlerHasCtorParamsToResolve_ParamsResolvedCorrectly

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
        public void HandleCommand_CommandHandlerHasCtorParamsToResolve_ParamsResolvedCorrectly()
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

        #region HandleCommand_CommandHandlerDoesNotExist_ShouldRaiseCommandHandlerNotFoundException

        private class Ns09_CommandWithNoHandler
        {
        }

        [Fact]
        public void HandleCommand_CommandHandlerDoesNotExist_ShouldRaiseCommandHandlerNotFoundException()
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

        #region HandleCommand_CommandHandlerHasClassSuffixSearchMethod_CommandHandlerIsFound

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
        public void HandleCommand_CommandHandlerHasClassSuffixSearchMethod_CommandHandlerIsFound()
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

        #region HandleCommand_HandlerMethodWithException_ShouldRaiseMessageProcessingException

        private class Ns11_CommandWithFail
        {
            public void Handle(Ns11_CommandWithFail ns11Command)
            {
                throw new NotImplementedException("Test exception.");
            }
        }

        [Fact]
        public void HandleCommand_HandlerMethodWithException_ShouldRaiseMessageProcessingException()
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

        #region HandleCommand_ServiceProvideNotSet_DependenciesShouldNotBeResolved

        public class Ns13_Command
        {
        }

        [CommandHandlers]
        public class Ns13_CommandHandlersWithDeps
        {
            public Ns13_CommandHandlersWithDeps(object dep)
            {
            }

            public void Handle(Ns13_Command ns13Command, Ns01_IInterfaceA dep1, Ns01_IInterfaceB dep2)
            {
                if (dep1 != null || dep2 != null)
                {
                    throw new InvalidOperationException("Something went wrong.");
                }
            }
        }

        [Fact]
        public void HandleCommand_ServiceProvideNotSet_DependenciesShouldNotBeResolved()
        {
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline();
            SetupCommandPipeline(builder);

            // Act & assert
            pipelineService.HandleCommand(new Ns13_Command());
        }

        #endregion

        #region HandleCommand_CommandPipelineWithUseExceptionDispatchInfo_ShouldRaiseOriginalException

        public class Ns14_Exception : Exception
        {
        }

        public class Ns14_SimpleTestCommand
        {
            public void Handle()
            {
                throw new Ns14_Exception();
            }
        }

        [Fact]
        public void HandleCommand_CommandPipelineWithUseExceptionDispatchInfo_ShouldRaiseOriginalException()
        {
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline()
                .AddStandardMiddlewares(options =>
                {
                    options.Assemblies =
                        new[] { typeof(CommandsTests).GetTypeInfo().Assembly };
                    options.UseExceptionDispatchInfo = true;
                });

            // Act & assert
            Assert.Throws<Ns14_Exception>(() =>
            {
                pipelineService.HandleCommand(new Ns14_SimpleTestCommand());
            });
        }

        #endregion

        #region HandleCommandAsync_HandleCommandWithAdditionalParam_ParamInMessageContextItems

        private class Ns15_TestParamMiddleware : IMessagePipelineMiddleware
        {
            public int HasParamCount { get; set; }

            public string Id { get; } = nameof(Ns15_TestParamMiddleware);

            public void Handle(IMessageContext messageContext)
            {
                if (messageContext.Items.ContainsKey(MessageContextConstants.ParamKey))
                {
                    HasParamCount++;
                }
            }
        }

        private class Ns15_EmptyCommand
        {
        }

        [Fact]
        public async void HandleCommandAsync_HandleCommandWithAdditionalParam_ParamInMessageContextItems()
        {
            // Arrange
            var testParamMiddleware = new Ns15_TestParamMiddleware();
            var builder = pipelineService.PipelineContainer.AddCommandPipeline()
                .AddMiddleware(testParamMiddleware);

            // Act
            pipelineService.HandleCommand(new Ns15_EmptyCommand());
            pipelineService.HandleCommand(new Ns15_EmptyCommand(), 1);
            await pipelineService.HandleCommandAsync(new Ns15_EmptyCommand());
            await pipelineService.HandleCommandAsync(new Ns15_EmptyCommand(), 1);

            // Assert
            Assert.Equal(2, testParamMiddleware.HasParamCount);
        }

        #endregion

        #region HandleCommandAsync_HandleCommandWithCancellationToken_CancellationTokenIsPassedToHandler

        private class Ns16_EmptyCommand
        {
            public CancellationToken CancellationToken { get; set; }

            public Task Handle(Ns16_EmptyCommand command, CancellationToken ct = default(CancellationToken))
            {
                CancellationToken = ct;
                return Task.FromResult(1);
            }
        }

        [Fact]
        public async void HandleCommandAsync_HandleCommandWithCancellationToken_CancellationTokenIsPassedToHandler()
        {
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline()
                .AddStandardMiddlewares(options =>
                {
                    options.Assemblies =
                        new[] { typeof(CommandsTests).GetTypeInfo().Assembly };
                });

            // Act
            var cts = new CancellationTokenSource();
            var command = new Ns16_EmptyCommand();
            await pipelineService.HandleCommandAsync(command, cts.Token);

            // Assert
            Assert.Equal(cts.Token, command.CancellationToken);
            cts.Dispose();
        }

        #endregion

        #region HandleCommandAsync_CommandWithException_ShouldRaiseOriginalException

        private class Ns17_ExceptionCommand
        {
            public Task Handle(Ns17_ExceptionCommand command)
            {
                throw new Ns17_Exception();
            }
        }

        private class Ns17_Exception : Exception
        {
        }

        [Fact]
        public async Task HandleCommandAsync_CommandWithException_ShouldRaiseOriginalException()
        {
            // Arrange
            var builder = pipelineService.PipelineContainer.AddCommandPipeline()
                .AddStandardMiddlewares(options =>
                {
                    options.Assemblies = new[] { typeof(CommandsTests).GetTypeInfo().Assembly };
                    options.UseExceptionDispatchInfo = true;
                });

            // Act & assert
            await Assert.ThrowsAsync<Ns17_Exception>(async () =>
            {
                await pipelineService.HandleCommandAsync(new Ns17_ExceptionCommand());
            });
        }

        #endregion
    }
}
