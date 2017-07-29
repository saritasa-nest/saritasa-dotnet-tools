// Copyright (c) 2015-2017, Saritasa. All rights reserved.
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

        #region Should dispose command handlers after instaniate if use internal object resolver

        [Fact]
        public void Should_dispose_command_handlers_after_instaniate_if_use_internal_object_resolver()
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
        public async Task Should_dispose_command_handlers_after_instaniate_if_use_internal_object_resolver_async()
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

        #region Should dispose query handlers after instaniate if use internal object resolver

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
        public void Should_dispose_query_handlers_after_instaniate_if_use_internal_object_resolver()
        {
            // Arrange
            pipelineService.PipelineContainer.AddQueryPipeline()
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware
                {
                    UseInternalObjectResolver = true,
                    UseParametersResolve = true
                })
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware())
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectReleaseMiddleware());

            // Act
            pipelineService.Query<QueryClass>().With(q => q.Query());

            // Assert
            Assert.True(QueryClass.IsDisposed);
        }

        #endregion

        #region Should not dispose query handlers after instaniate if not use internal object resolver

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
        public void Should_not_dispose_query_handlers_after_instaniate_if_not_use_internal_object_resolver()
        {
            // Arrange
            pipelineService.PipelineContainer.AddQueryPipeline()
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware
                {
                    UseInternalObjectResolver = false,
                    UseParametersResolve = true
                })
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware())
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectReleaseMiddleware());

            // Act
            pipelineService.ServiceProvider = new FuncServiceProvider(Activator.CreateInstance);
            pipelineService.Query<QueryClass>().With(q => q.Query());

            // Assert
            Assert.False(QueryClass.IsDisposed);
        }

        #endregion

        #region Should not dispose command handlers after instaniate if not use internal object resolver

        [Fact]
        public void Should_not_dispose_command_handlers_after_instaniate_if_not_use_internal_object_resolver()
        {
            // Arrange
            pipelineService.PipelineContainer.AddCommandPipeline()
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CommandsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandExecutorMiddleware
                {
                    UseInternalObjectResolver = false,
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
