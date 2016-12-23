// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Events
{
    using System;
    using System.Reflection;
    using Abstractions;
    using Common;

    /// <summary>
    /// Events pipeline.
    /// </summary>
    public class EventPipeline : MessagePipeline, IEventPipeline
    {
        static readonly byte[] AvailableMessageTypes = new byte[] { Message.MessageTypeEvent };

        /// <inheritdoc />
        public override byte[] MessageTypes => AvailableMessageTypes;

        /// <inheritdoc />
        public void Raise(object @event)
        {
            ProcessMiddlewares(new EventMessage(@event));
        }

        /// <summary>
        /// Creates default pipeline with events executor.
        /// </summary>
        /// <returns>Event pipeline.</returns>
        public static IEventPipeline CreateDefaultPipeline(Func<Type, object> resolver, params Assembly[] assemblies)
        {
            var eventPipeline = new EventPipeline();
            if (assemblies == null || assemblies.Length < 1)
            {
                assemblies = new[] { Assembly.GetEntryAssembly() };
            }

            eventPipeline.AppendMiddlewares(
                new PipelineMiddlewares.EventHandlerLocatorMiddleware(assemblies),
                new PipelineMiddlewares.EventExecutorMiddleware(resolver)
            );
            return eventPipeline;
        }

#pragma warning disable SA1401
        /// <summary>
        /// Empty event pipeline;
        /// </summary>
        public static readonly EventPipeline Empty = new EventPipeline();
#pragma warning restore SA1401

        /// <inheritdoc />
        public override void ProcessRaw(IMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
