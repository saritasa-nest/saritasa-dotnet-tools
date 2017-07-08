// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Events specific pipeline.
    /// </summary>
    public interface IEventPipeline : IMessagePipeline
    {
        /// <summary>
        /// Raise event.
        /// </summary>
        /// <param name="event">Event to raise.</param>
        void Raise([NotNull] object @event);

        /// <summary>
        /// Raise event asynchronously.
        /// </summary>
        /// <param name="event">Event to raise.</param>
        Task RaiseAsync([NotNull] object @event);
    }
}
