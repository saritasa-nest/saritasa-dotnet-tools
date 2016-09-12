// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Events
{
    using System;
    using System.Reflection;
    using Messages;

    /// <summary>
    /// Events pipeline.
    /// </summary>
    public class EventPipeline : MessagePipeline, IEventPipeline
    {
        static readonly byte[] AvailableMessageTypes = new byte[] { Message.MessageTypeEvent };

        /// <inheritdoc />
        public override byte[] MessageTypes => AvailableMessageTypes;

        private void ProcessPipeline(EventMessage message)
        {
            foreach (var handler in Middlewares)
            {
                handler.Handle(message);
            }
        }

        /// <inheritdoc />
        public void Raise(object @event)
        {
            var eventMessage = new EventMessage(@event);
            foreach (var handler in Middlewares)
            {
                handler.Handle(eventMessage);
            }
        }

        /// <summary>
        /// Creates default pipeline with events executor.
        /// </summary>
        /// <returns>Event pipeline.</returns>
        public static EventPipeline CreateDefaultPipeline(Func<Type, object> resolver, params Assembly[] assemblies)
        {
            var eventPipeline = new EventPipeline();
            if (assemblies == null || assemblies.Length < 1)
            {
                assemblies = new Assembly[] { Assembly.GetEntryAssembly() };
            }

            eventPipeline.AppendMiddlewares(
                new EventPipelineMiddlewares.EventHandlerLocatorMiddleware(assemblies),
                new EventPipelineMiddlewares.EventExecutorMiddleware(resolver)
            );
            return eventPipeline;
        }

        /// <inheritdoc />
        public override void ProcessRaw(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
