// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using System.Reflection;
    using NUnit.Framework;
    using Domain;
    using Events;
    using Events.EventPipelineMiddlewares;

    [TestFixture]
    public class EventsTests
    {
        #region Interfaces

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

        [Test]
        public void Events_should_be_fired_withing_all_classes()
        {
            var ep = EventPipeline.CreateDefaultPipeline(InterfacesResolver,
                Assembly.GetAssembly(typeof(CommandsTests))).UseInternalResolver(true);
            var ev = new CreateUserEvent()
            {
                UserId = 10,
                FirstName = "Ivan",
                LastName = "Ivanov",
            };
            ep.Raise(ev);
            Assert.That(ev.HandlersCount, Is.EqualTo(3));
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

        [Test]
        public void Domain_events_can_be_integrated_to_events_pipeline()
        {
            var eventsManager = new DomainEventsManager();
            Func<Type, object> resolver = (type) =>
            {
                if (type == typeof(IDomainEventsManager))
                {
                    return eventsManager;
                }
                return null;
            };
            var ep = EventPipeline.CreateDefaultPipeline(resolver, Assembly.GetAssembly(typeof(CommandsTests)));
            eventsManager.Register(new DomainTestEventHandler());
            ep.InsertMiddlewareBefore(new DomainEventLocatorMiddleware(eventsManager), "EventLocator");

            var ev = new DomainTestEvent();
            ep.Raise(ev);
            Assert.That(ev.Param, Is.EqualTo(42));
        }

        #endregion
    }
}
