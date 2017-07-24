// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
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
        private readonly IPipelineService pipelinesService = new DefaultPipelineService();

        #region Shared interfaces

        public interface IInterfaceA
        {
            string GetTestValue();
        }

        public class ImplementationA : IInterfaceA
        {
            public string GetTestValue() => "A";
        }

        public static object InterfacesResolver(Type t)
        {
            if (t == typeof(IInterfaceA))
            {
                return new ImplementationA();
            }
            return null;
        }

        #endregion

        #region Shared events and handlers

        class CreateUserEvent
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
        class UserEventHandlers
        {
            public IInterfaceA InterfaceA1 { get; set; }

            public void Handle(CreateUserEvent @event, IInterfaceA interfaceA2)
            {
                if (InterfaceA1 != null && interfaceA2 != null)
                {
                    @event.HandlersCount++;
                }
            }
        }

        [EventHandlers]
        class UserSimpleHandler
        {
            public void HandleOnUserCreate(CreateUserEvent @event)
            {
                @event.HandlersCount++;
            }
        }

        #endregion

        private void SetupEventPipeline(EventPipelineBuilder builder)
        {
            builder
            .AddMiddleware(new Events.PipelineMiddlewares.EventHandlerLocatorMiddleware(
                typeof(EventsTests).GetTypeInfo().Assembly))
            .AddMiddleware(new Events.PipelineMiddlewares.EventExecutorMiddleware
            {
                UseInternalObjectResolver = true,
                UseParametersResolve = true
            });
        }

        [Fact]
        public void Events_should_be_fired_withing_all_classes()
        {
            // Arrange
            pipelinesService.ServiceProvider = new FuncServiceProvider(InterfacesResolver);
            SetupEventPipeline(pipelinesService.PipelineContainer.AddEventPipeline());
            var ev = new CreateUserEvent
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

        private class DomainTestEvent
        {
            public int Param { get; set; }
        }

        private class DomainTestEventHandler : IDomainEventHandler<DomainTestEvent>
        {
            public void Handle(DomainTestEvent @event)
            {
                @event.Param = 42;
            }
        }

        [Fact]
        public void Domain_events_can_be_integrated_to_events_pipeline()
        {
            // Arrange
            var eventsManager = new DomainEventsManager();
            eventsManager.Register(new DomainTestEventHandler());
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
                .AddMiddleware(new Events.PipelineMiddlewares.EventExecutorMiddleware
                {
                    UseInternalObjectResolver = true,
                    UseParametersResolve = true
                });
            var ev = new DomainTestEvent();

            // Act
            pipelinesService.RaiseEvent(ev);

            // Assert
            Assert.Equal(42, ev.Param);
        }

        #endregion
    }
}
