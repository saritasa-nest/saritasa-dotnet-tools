// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    /// <summary>
    /// Domain events manager in-memory implementation.
    /// </summary>
    public class DomainEventsManager : IDomainEventsManager
    {
        readonly List<object> handlers = new List<object>();

        /// <summary>
        /// List of handlers.
        /// </summary>
        protected List<object> Handlers => handlers;

        /// <inheritdoc />
        public virtual void Register<TEvent>(
            [NotNull] IDomainEventHandler<TEvent> handler) where TEvent : class
        {
            Handlers.Add(handler);
        }

        /// <inheritdoc />
        public virtual void Raise<TEvent>([NotNull] TEvent @event) where TEvent : class
        {
            var registeredHandlers = handlers.Where(h => h is IDomainEventHandler<TEvent>);
            foreach (IDomainEventHandler<TEvent> handler in registeredHandlers)
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

        /// <inheritdoc />
        public bool HasHandlers<TEvent>() where TEvent : class
        {
            return handlers.Any(h => h is IDomainEventHandler<TEvent>);
        }
    }
}
