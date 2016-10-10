// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using NUnit.Framework;
    using Commands;
    using Commands.CommandPipelineMiddlewares;
    using Messages;

    [TestFixture]
    public class MessagesTests
    {
        #region Test middlewares

        class TestMiddleware1 : IMessagePipelineMiddleware
        {
            public string Id => "Test1";

            public void Handle(Message message)
            {
            }
        }

        class TestMiddleware2 : IMessagePipelineMiddleware
        {
            public string Id => "Test2";

            public void Handle(Message message)
            {
            }
        }

        class TestMiddleware3 : IMessagePipelineMiddleware
        {
            public string Id => "Test3";

            public void Handle(Message message)
            {
            }
        }

        #endregion

        [Test]
        public void Messages_pipeline_insert_after_should_increase_length()
        {
            var cp = new CommandPipeline();
            cp.AppendMiddlewares(new CommandHandlerLocatorMiddleware(Assembly.GetAssembly(typeof(MessagesTests))));
            Assert.That(cp.GetMiddlewares().Count(), Is.EqualTo(1), "AppendMiddlewares failed");

            cp.InsertMiddlewareAfter(new TestMiddleware1());
            Assert.That(cp.GetMiddlewares().ElementAt(1).Id, Is.EqualTo("Test1"), "InsertMiddlewareAfter1 failed");
            cp.InsertMiddlewareAfter(new TestMiddleware3());

            cp.InsertMiddlewareAfter(new TestMiddleware2(), "Test1");
            Assert.That(cp.GetMiddlewares().ElementAt(2).Id, Is.EqualTo("Test2"), "InsertMiddlewareAfter2 failed");
            Assert.That(cp.GetMiddlewares().ElementAt(3).Id, Is.EqualTo("Test3"), "InsertMiddlewareAfter2 failed");
            Assert.That(cp.GetMiddlewares().Count(), Is.EqualTo(4));
        }

        [Test]
        public void Messages_pipeline_insert_before_should_increase_length()
        {
            var cp = new CommandPipeline();
            cp.AppendMiddlewares(new CommandHandlerLocatorMiddleware(Assembly.GetAssembly(typeof(MessagesTests))));
            Assert.That(cp.GetMiddlewares().Count(), Is.EqualTo(1), "AppendMiddlewares failed");

            cp.InsertMiddlewareBefore(new TestMiddleware1());
            Assert.That(cp.GetMiddlewares().ElementAt(0).Id, Is.EqualTo("Test1"), "InsertMiddlewareBefore1 failed");
            cp.InsertMiddlewareAfter(new TestMiddleware2());

            // test1, locator, test3, test2
            cp.InsertMiddlewareBefore(new TestMiddleware3(), "Test2");
            Assert.That(cp.GetMiddlewares().ElementAt(2).Id, Is.EqualTo("Test3"), "InsertMiddlewareBefore2 failed");
            Assert.That(cp.GetMiddlewares().ElementAt(3).Id, Is.EqualTo("Test2"), "InsertMiddlewareBefore2 failed");
            Assert.That(cp.GetMiddlewares().Count(), Is.EqualTo(4));
        }

        [Test]
        public void Inserting_middlewares_with_duplicated_ids_should_generate_exception()
        {
            var cp = new CommandPipeline();
            cp.AppendMiddlewares(new CommandHandlerLocatorMiddleware(Assembly.GetAssembly(typeof(MessagesTests))));
            cp.InsertMiddlewareBefore(new TestMiddleware1());

            bool fired = false;
            try
            {
                cp.InsertMiddlewareBefore(new TestMiddleware1());
            }
            catch (ArgumentException)
            {
                fired = true;
            }
            Assert.That(fired, Is.True);
        }
    }
}
