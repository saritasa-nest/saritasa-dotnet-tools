// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Events;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Events;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// Message events tests.
    /// </summary>
    public class EventsTests
    {
        private readonly IMessagePipelineService pipelinesService = new DefaultMessagePipelineService();

        #region Shared interfaces

        public interface Ns01_IInterfaceA
        {
            string GetTestValue();
        }

        public class Ns01_ImplementationA : Ns01_IInterfaceA
        {
            public string GetTestValue() => "A";
        }

        public static object Ns01_InterfacesResolver(Type t)
        {
            if (t == typeof(Ns01_IInterfaceA))
            {
                return new Ns01_ImplementationA();
            }
            return null;
        }

        #endregion

        #region Shared events and handlers

        private class Ns01_CreateUserEvent
        {
            public int UserId { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int HandlersCount { get; set; }

            public void Handle()
            {
                HandlersCount++;
            }
        }

        [EventHandlers]
        private class Ns01_UserEventHandlers
        {
            public Ns01_IInterfaceA Ns01IInterfaceA1 { get; set; }

            public void Handle(Ns01_CreateUserEvent @event, Ns01_IInterfaceA ns01IInterfaceA2)
            {
                if (Ns01IInterfaceA1 != null && ns01IInterfaceA2 != null)
                {
                    @event.HandlersCount++;
                }
            }
        }

        [EventHandlers]
        private class Ns01_UserSimpleHandler
        {
            public void HandleOnUserCreate(Ns01_CreateUserEvent @event)
            {
                @event.HandlersCount++;
            }
        }

        private void SetupEventPipeline(EventPipelineBuilder builder)
        {
            builder
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerLocatorMiddleware(
                    typeof(EventsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerResolverMiddleware())
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });
        }

        #endregion

        #region Events should be fired withing all classes

        [Fact]
        public void Events_should_be_fired_withing_all_classes()
        {
            // Arrange
            pipelinesService.ServiceProvider = new FuncServiceProvider(Ns01_InterfacesResolver);
            pipelinesService.PipelineContainer.AddEventPipeline()
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerLocatorMiddleware(
                    typeof(EventsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerResolverMiddleware
                {
                    UsePropertiesResolving = true
                })
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });
            var ev = new Ns01_CreateUserEvent
            {
                UserId = 10,
                FirstName = "Ivan",
                LastName = "Ivanov",
            };

            // Act
            pipelinesService.RaiseEvent(ev);

            // Assert
            Assert.Equal(3, ev.HandlersCount);
        }

        #endregion

        #region Can run default simple generic command

        public class Ns04_SimpleGenericEvent<T>
        {
            public T Id { get; set; }

            public string Out { get; set; }
        }

        [EventHandlers]
        public class Ns03_TestEventHandler
        {
            public void HandleTestEvent1<T>(Ns04_SimpleGenericEvent<T> command)
            {
                command.Out += "1";
            }

            public void HandleTestEvent2<T>(Ns04_SimpleGenericEvent<T> command)
            {
                command.Out += "1";
            }
        }

        [Fact]
        public void Can_run_default_simple_generic_command()
        {
            // Arrange
            SetupEventPipeline(pipelinesService.PipelineContainer.AddEventPipeline());

            // Act
            var ev = new Ns04_SimpleGenericEvent<string>
            {
                Id = "GameOfThrones"
            };
            pipelinesService.RaiseEvent(ev);

            // Assert
            Assert.Equal("GameOfThrones", ev.Id);
            Assert.Equal("11", ev.Out);
        }

        #endregion

        #region Async event should bypass cancellation token

        public class Ns05_AsyncEvent
        {
            public CancellationToken CancellationToken { get; set; }
        }

        [EventHandlers]
        private class Ns05_AsyncEventHandler
        {
            public Task Handle(Ns05_AsyncEvent ev, CancellationToken token = default(CancellationToken))
            {
                ev.CancellationToken = token;
                return Task.FromResult(1);
            }
        }

        [Fact]
        public async void Async_event_should_bypass_cancellation_token()
        {
            // Arrange
            pipelinesService.PipelineContainer.AddEventPipeline().AddStandardMiddlewares(options =>
            {
                options.Assemblies =
                    new List<Assembly> { typeof(EventsTests).GetTypeInfo().Assembly };
            });

            // Act
            var cts = new CancellationTokenSource();
            var ev = new Ns05_AsyncEvent();
            await pipelinesService.RaiseEventAsync(ev, cts.Token);

            // Assert
            Assert.Equal(cts.Token, ev.CancellationToken);
        }

        #endregion

        #region Event with no handlers should do nothing

        public class Ns06_NoHandlerEvent
        {
            public CancellationToken CancellationToken { get; set; }
        }

        [Fact]
        public void Event_with_no_handlers_should_do_nothing()
        {
            // Arrange
            pipelinesService.PipelineContainer.AddEventPipeline().AddStandardMiddlewares(options =>
            {
                options.Assemblies =
                    new List<Assembly> { typeof(EventsTests).GetTypeInfo().Assembly };
            });

            // Act & Assert
            pipelinesService.RaiseEvent(new Ns06_NoHandlerEvent());
        }

        #endregion
    }
}
