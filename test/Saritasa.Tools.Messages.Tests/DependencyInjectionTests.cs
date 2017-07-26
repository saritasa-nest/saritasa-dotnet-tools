// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Commands;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// Dependency injection tests.
    /// </summary>
    public class DependencyInjectionTests
    {
        private readonly IMessagePipelineService pipelineService = new DefaultMessagePipelineService();

        public interface IInterfaceA
        {
            string GetTestValue();
        }

        public class ImplementationA : IInterfaceA, IDisposable
        {
            public string GetTestValue() => "A";

            public static bool IsDisposed { get; set; }

            public ImplementationA()
            {
                IsDisposed = false;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public static object InterfacesResolver(Type t)
        {
            if (t == typeof(IInterfaceA))
            {
                return new ImplementationA();
            }
            return null;
        }

        #region Should dispose command handlers after instaniate

        public class TestCommand
        {
        }

        [CommandHandlers]
        public class TestCommandHandler : IDisposable
        {
            public static bool IsDisposed { get; set; }

            public void HandleTestCommand(TestCommand command)
            {
                IsDisposed = false;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Fact]
        public void Should_dispose_command_handlers_after_instaniate_if_self_created()
        {
            // Arrange
            pipelineService.PipelineContainer.AddCommandPipeline()
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CommandsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandExecutorMiddleware
                {
                    UseInternalObjectResolver = true,
                    UseParametersResolve = true
                });
            var cmd = new TestCommand();

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.True(TestCommandHandler.IsDisposed);
        }

        [Fact]
        public async Task Should_dispose_command_handlers_after_instaniate_async()
        {
            // Arrange
            pipelineService.PipelineContainer.AddCommandPipeline()
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CommandsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandExecutorMiddleware
                {
                    UseInternalObjectResolver = true,
                    UseParametersResolve = true
                });
            var cmd = new TestCommand();

            // Act
            await pipelineService.HandleCommandAsync(cmd);

            // Assert
            Assert.True(TestCommandHandler.IsDisposed);
        }

        #endregion
    }
}
