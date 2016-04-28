//
// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
//

namespace Saritasa.Tools.Interfaces
{
    /// <summary>
    /// Interface for event handlers.
    /// </summary>
    /// <typeparam name="T">Domain event type.</typeparam>
    public interface IDomainEventHandler<T> where T : IDomainEvent
    {
        /// <summary>
        /// Handle (call methods) for domain event of type T.
        /// </summary>
        /// <param name="event">Domain event type.</param>
        void Handle(T @event);
    }
}
