// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Tests
{
    using System;
    using System.Reflection;
    using Xunit;
    using Domain;
    using Abstractions;
    using Events;
    using Events.PipelineMiddlewares;

    /// <summary>
    /// Message events tests.
    /// </summary>
    public class EventsTests
    {
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

        [Fact]
        public void Events_should_be_fired_withing_all_classes()
        {
            // Arrange
            var ep = EventPipeline.CreateDefaultPipeline(InterfacesResolver,
                typeof(CommandsTests).GetTypeInfo().Assembly).UseInternalResolver();
            var ev = new CreateUserEvent()
            {
                UserId = 10,
                FirstName = "Ivan",
                LastName = "Ivanov",
            };

            // Act
            ep.Raise(ev);

            // Assert
            Assert.Equal(3, ev.HandlersCount);
        }

        #region Domain_events_can_be_integrated_to_events_pipeline

        class DomainTestEvent
        {
            public int Param { get; set; }
        }

        class DomainTestEventHandler : IDomainEventHandler<DomainTestEvent>
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
            Func<Type, object> resolver = (type) =>
            {
                if (type == typeof(IDomainEventsManager))
                {
                    return eventsManager;
                }
                return null;
            };
            var ep = EventPipeline.CreateDefaultPipeline(resolver, typeof(CommandsTests).GetTypeInfo().Assembly);
            eventsManager.Register(new DomainTestEventHandler());
            ep.InsertMiddlewareBefore(new DomainEventLocatorMiddleware(eventsManager), "EventLocator");
            var ev = new DomainTestEvent();

            // Act
            ep.Raise(ev);

            // Assert
            Assert.Equal(42, ev.Param);
        }

        #endregion
    }
}
