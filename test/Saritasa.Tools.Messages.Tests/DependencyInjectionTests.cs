// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Commands;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Queries;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// Dependency injection tests.
    /// </summary>
    public class DependencyInjectionTests
    {
        private readonly IMessagePipelineService pipelineService = new DefaultMessagePipelineService();

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

        #region HandleCommand_CommandHandlerIsDisposableAndInternalObjectResolvedUsed_DisposeMethodIsCalled

        [Fact]
        public void HandleCommand_CommandHandlerIsDisposableAndInternalObjectResolvedUsed_DisposeMethodIsCalled()
        {
            // Arrange
            pipelineService.PipelineContainer.AddCommandPipeline()
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CommandsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerResolverMiddleware())
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });
            var cmd = new TestCommand();

            // Act
            pipelineService.HandleCommand(cmd);

            // Assert
            Assert.True(TestCommandHandler.IsDisposed);
        }

        [Fact]
        public async Task Should_dispose_command_handlers_after_instaniate_if_use_internal_object_resolver_async()
        {
            // Arrange
            pipelineService.PipelineContainer.AddCommandPipeline()
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CommandsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerResolverMiddleware())
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });
            var cmd = new TestCommand();

            // Act
            await pipelineService.HandleCommandAsync(cmd);

            // Assert
            Assert.True(TestCommandHandler.IsDisposed);
        }

        #endregion

        #region QueryWith_QueryIsDisposable_DisposeMethodIsCalled

        [QueryHandlers]
        public class QueryClass : IDisposable
        {
            public static bool IsDisposed { get; set; }

            public int Query() => 42;

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Fact]
        public void QueryWith_QueryIsDisposable_DisposeMethodIsCalled()
        {
            // Arrange
            pipelineService.PipelineContainer.AddQueryPipeline()
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware())
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware());

            // Act
            pipelineService.Query<QueryClass>().With(q => q.Query());

            // Assert
            Assert.True(QueryClass.IsDisposed);
        }

        #endregion

        #region QueryWith_QueryIsDisposableButInternalObjectResolverIsNotUsed_DisposeMethodIsNotCalled

        [QueryHandlers]
        public class QueryClass2 : IDisposable
        {
            public static bool IsDisposed { get; set; }

            public int Query() => 42;

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Fact]
        public void QueryWith_QueryIsDisposableButInternalObjectResolverIsNotUsed_DisposeMethodIsNotCalled()
        {
            // Arrange
            pipelineService.PipelineContainer.AddQueryPipeline()
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware(false))
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware());

            // Act
            pipelineService.ServiceProvider = new FuncServiceProvider(Activator.CreateInstance);
            pipelineService.Query<QueryClass2>().With(q => q.Query());

            // Assert
            Assert.False(QueryClass2.IsDisposed);
        }

        #endregion

        #region HandleCommand_CommandHandlerIsDisposableButInternalObjectResolverIsNotUsed_DisposeMethodIsNotCalled

        [Fact]
        public void HandleCommand_CommandHandlerIsDisposableButInternalObjectResolverIsNotUsed_DisposeMethodIsNotCalled()
        {
            // Arrange
            pipelineService.PipelineContainer.AddCommandPipeline()
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CommandsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerResolverMiddleware(false))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });
            var cmd = new TestCommand();

            // Act
            pipelineService.ServiceProvider = new FuncServiceProvider(Activator.CreateInstance);
            pipelineService.HandleCommand(cmd);

            // Assert
            // Since handler object is controlled by external resolver we should not dispose it ourself.
            Assert.False(TestCommandHandler.IsDisposed);
        }

        #endregion
    }
}
