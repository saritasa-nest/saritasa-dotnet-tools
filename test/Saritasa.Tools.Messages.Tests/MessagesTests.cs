// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using Abstractions;
    using Common;
    using Commands;
    using Commands.PipelineMiddlewares;

    public class MessagesTests
    {
        #region Test middlewares

        class TestMiddleware1 : IMessagePipelineMiddleware
        {
            public string Id => "Test1";

            public void Handle(IMessage message)
            {
            }
        }

        class TestMiddleware2 : IMessagePipelineMiddleware
        {
            public string Id => "Test2";

            public void Handle(IMessage message)
            {
            }
        }

        class TestMiddleware3 : IMessagePipelineMiddleware
        {
            public string Id => "Test3";

            public void Handle(IMessage message)
            {
            }
        }

        #endregion

        [Fact]
        public void Messages_pipeline_insert_after_should_increase_length()
        {
            // Arrange
            var cp = new CommandPipeline();

            // Acts
            cp.AppendMiddlewares(new CommandHandlerLocatorMiddleware(typeof(MessagesTests).GetTypeInfo().Assembly));
            var countAfterAppend = cp.GetMiddlewares().Count();
            cp.InsertMiddlewareAfter(new TestMiddleware1());
            var middlewareId = cp.GetMiddlewares().ElementAt(1).Id;
            cp.InsertMiddlewareAfter(new TestMiddleware3());
            cp.InsertMiddlewareAfter(new TestMiddleware2(), "Test1");

            // Assert
            Assert.Equal(1, countAfterAppend);
            Assert.Equal("Test1", middlewareId);
            Assert.Equal("Test2", cp.GetMiddlewares().ElementAt(2).Id);
            Assert.Equal("Test3", cp.GetMiddlewares().ElementAt(3).Id);
            Assert.Equal(4, cp.GetMiddlewares().Count());
        }

        [Fact]
        public void Messages_pipeline_insert_before_should_increase_length()
        {
            // Arrange
            var cp = new CommandPipeline();

            // Acts
            cp.AppendMiddlewares(new CommandHandlerLocatorMiddleware(typeof(MessagesTests).GetTypeInfo().Assembly));
            var countAfterAppend = cp.GetMiddlewares().Count();
            cp.InsertMiddlewareBefore(new TestMiddleware1());
            var middlewareId = cp.GetMiddlewares().ElementAt(0).Id;
            cp.InsertMiddlewareAfter(new TestMiddleware2());
            cp.InsertMiddlewareBefore(new TestMiddleware3(), "Test2"); // test1, locator, test3, test2

            // Assert
            Assert.Equal(1, countAfterAppend);
            Assert.Equal("Test1", middlewareId);
            Assert.Equal("Test3", cp.GetMiddlewares().ElementAt(2).Id);
            Assert.Equal("Test2", cp.GetMiddlewares().ElementAt(3).Id);
            Assert.Equal(4, cp.GetMiddlewares().Count());
        }

        [Fact]
        public void Inserting_middlewares_with_duplicated_ids_should_generate_exception()
        {
            // Arrange
            var cp = new CommandPipeline();
            cp.AppendMiddlewares(new CommandHandlerLocatorMiddleware(typeof(MessagesTests).GetTypeInfo().Assembly));
            cp.InsertMiddlewareBefore(new TestMiddleware1());

            // Act & assert
            Assert.Throws<ArgumentException>(() => { cp.InsertMiddlewareBefore(new TestMiddleware1()); });
        }
    }
}
