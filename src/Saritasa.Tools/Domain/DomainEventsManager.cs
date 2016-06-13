// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class for manage events.
    /// </summary>
    public class DomainEventsManager : IDomainEventsManager
    {
        private List<object> handlers = new List<object>();

        private List<object> Handlers
        {
            get
            {
                return handlers;
            }
        }

        /// <inheritdoc />
        public void Register<T>(IDomainEventHandler<T> handler) where T : IDomainEvent
        {
            Handlers.Add(handler);
        }

        /// <inheritdoc />
        public void Raise<T>(T @event) where T : IDomainEvent
        {
            var registeredHandlers = handlers.Where(h => h is IDomainEventHandler<T>);
            foreach (IDomainEventHandler<T> handler in registeredHandlers)
            {
                handler.Handle(@event);
            }
        }

        /// <summary>
        /// Remove all handlers.
        /// </summary>
        public void Clear()
        {
            handlers.Clear();
        }
    }
}
