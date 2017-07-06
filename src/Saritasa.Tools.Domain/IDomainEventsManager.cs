// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// Interface for manage events.
    /// </summary>
    public interface IDomainEventsManager
    {
        /// <summary>
        /// Register the domain event handler.
        /// </summary>
        /// <typeparam name="TEvent">Event.</typeparam>
        /// <param name="handler">Event handler.</param>
        void Register<TEvent>(
            IDomainEventHandler<TEvent> handler) where TEvent : class;

        /// <summary>
        /// Raise the domain event.
        /// </summary>
        /// <typeparam name="TEvent">Event type.</typeparam>
        /// <param name="event">Event.</param>
        void Raise<TEvent>(TEvent @event) where TEvent : class;

        /// <summary>
        /// Determines whether events manager has handlers for specific event.
        /// </summary>
        /// <typeparam name="TEvent">Event type.</typeparam>
        /// <returns>True if event can be handled.</returns>
        bool HasHandlers<TEvent>() where TEvent : class;
    }
}
