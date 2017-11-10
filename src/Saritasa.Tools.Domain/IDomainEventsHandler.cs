// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// Interface for event handlers.
    /// </summary>
    /// <typeparam name="TEvent">Domain event type.</typeparam>
    public interface IDomainEventHandler<in TEvent> where TEvent : class
    {
        /// <summary>
        /// Handle (call methods) for domain event of type TEvent.
        /// </summary>
        /// <param name="event">Domain event type.</param>
        void Handle(TEvent @event);
    }
}
