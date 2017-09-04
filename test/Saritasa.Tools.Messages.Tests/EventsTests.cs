// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Saritasa.Tools.Domain;
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

        #region Domain_events_can_be_integrated_to_events_pipeline

        private class Ns02_DomainTestEvent
        {
            public int Param { get; set; }
        }

        private class Ns02_DomainTestEventHandler : IDomainEventHandler<Ns02_DomainTestEvent>
        {
            public void Handle(Ns02_DomainTestEvent @event)
            {
                @event.Param = 42;
            }
        }

        [Fact]
        public void Domain_events_can_be_integrated_to_events_pipeline()
        {
            // Arrange
            var eventsManager = new DomainEventsManager();
            eventsManager.Register(new Ns02_DomainTestEventHandler());
            object Resolver(Type type)
            {
                if (type == typeof(IDomainEventsManager))
                {
                    return eventsManager;
                }
                return null;
            }

            pipelinesService.ServiceProvider = new FuncServiceProvider(Resolver);
            pipelinesService.PipelineContainer.AddEventPipeline()
                .AddMiddleware(new Events.PipelineMiddlewares.DomainEventLocatorMiddleware(eventsManager))
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerResolverMiddleware())
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });
            var ev = new Ns02_DomainTestEvent();

            // Act
            pipelinesService.RaiseEvent(ev);

            // Assert
            Assert.Equal(42, ev.Param);
        }

        #endregion

        #region Can combine domain event with class events

        private class Ns03_DomainTestEvent
        {
        }

        private class Ns03_DomainTestEventHandler : IDomainEventHandler<Ns02_DomainTestEvent>
        {
            public void Handle(Ns02_DomainTestEvent @event)
            {
                Ns03_EventHandler.CallCount++;
            }
        }

        public class Ns03_Event1 { }

        public class Ns03_Event2 { }

        public class Ns03_Event3 { }

        [EventHandlers]
        public class Ns03_EventHandler
        {
            public static int CallCount = 0;

            public void Handle(Ns03_Event1 ev)
            {
                CallCount++;
            }

            public void Handle(Ns03_Event2 ev)
            {
                CallCount++;
            }
        }

        [EventHandlers]
        public class Ns03_EventHandler2
        {
            public void Handle(Ns03_Event3 ev)
            {
                Ns03_EventHandler.CallCount++;
            }
        }

        [Fact]
        public async Task Can_combine_domain_event_with_class_events()
        {
            // Arrange
            var eventsManager = new DomainEventsManager();
            eventsManager.Register(new Ns03_DomainTestEventHandler());

            pipelinesService.PipelineContainer.AddEventPipeline()
                .AddMiddleware(new Events.PipelineMiddlewares.DomainEventLocatorMiddleware(eventsManager))
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerLocatorMiddleware(
                    typeof(EventsTests).GetTypeInfo().Assembly))
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerResolverMiddleware())
                .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerExecutorMiddleware
                {
                    UseParametersResolve = true
                });

            // Act
            await pipelinesService.RaiseEventAsync(new Ns02_DomainTestEvent());
            await pipelinesService.RaiseEventAsync(new Ns03_Event1());
            await pipelinesService.RaiseEventAsync(new Ns03_Event2());
            await pipelinesService.RaiseEventAsync(new Ns03_Event3());

            // Assert
            Assert.Equal(4, Ns03_EventHandler.CallCount);
        }

        #endregion

        #region Can_run_default_simple_generic_command

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
    }
}
