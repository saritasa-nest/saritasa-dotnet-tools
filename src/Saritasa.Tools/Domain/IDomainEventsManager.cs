// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

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
        /// <typeparam name="T">Event.</typeparam>
        /// <param name="handler">Event handler.</param>
        void Register<T>(IDomainEventHandler<T> handler) where T : IDomainEvent;

        /// <summary>
        /// Raise the domain event.
        /// </summary>
        /// <typeparam name="T">Event type.</typeparam>
        /// <param name="event">Event.</param>
        void Raise<T>(T @event) where T : IDomainEvent;
    }
}
