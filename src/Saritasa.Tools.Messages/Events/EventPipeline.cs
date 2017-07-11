// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Events
{
    /// <summary>
    /// Events pipeline.
    /// </summary>
    public class EventPipeline : MessagePipeline, IEventPipeline
    {
        static readonly byte[] availableMessageTypes = new byte[] { Message.MessageTypeEvent };

        /// <inheritdoc />
        public override byte[] MessageTypes => availableMessageTypes;

        #region IEventPipeline

        /// <inheritdoc />
        public virtual void Raise(object @event)
        {
            ProcessMiddlewares(new EventMessage(@event));
        }

        /// <inheritdoc />
        public virtual async Task RaiseAsync(object @event, CancellationToken cancellationToken)
        {
            await ProcessMiddlewaresAsync(new EventMessage(@event), cancellationToken);
        }

        #endregion

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
