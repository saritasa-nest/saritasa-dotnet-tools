// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Events
{
    using System;
    using Common;

    /// <summary>
    /// Events specific pipeline.
    /// </summary>
    public interface IEventPipeline : IMessagePipeline
    {
        /// <summary>
        /// Raise event.
        /// </summary>
        /// <param name="event">Event to raise.</param>
        void Raise(object @event);
    }
}
