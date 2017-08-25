// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Commands.PipelineMiddlewares
{
    /// <summary>
    /// The middleware to resolve handler object. The resolved object will be stored to
    /// "handler-object" context items.
    /// </summary>
    public class CommandHandlerResolverMiddleware : IMessagePipelineMiddleware, IMessagePipelinePostAction
    {
        /// <summary>
        /// Handle object key.
        /// </summary>
        public const string HandlerObjectKey = "handler-object";

        /// <inheritdoc />
        public string Id { get; set; } = nameof(CommandHandlerResolverMiddleware);

        /// <summary>
        /// If <c>true</c> the middleware will resolve project using internal resolver. Default is <c>true</c>.
        /// </summary>
        public bool UseInternalObjectResolver { get; set; } = true;

        /// <inheritdoc />
        public void Handle(IMessageContext messageContext)
        {
            var handlerMethod = messageContext.GetItemByKey<MethodInfo>(CommandHandlerLocatorMiddleware.HandlerMethodKey);
            var type = handlerMethod.DeclaringType;

            // Rejected commands are not needed to process.
            if (messageContext.Status == ProcessingStatus.Rejected)
            {
                return;
            }

            object handler = null;

            // When command class contains Handle method within.
            if (handlerMethod.DeclaringType == messageContext.Content.GetType())
            {
                handler = messageContext.Content;
            }
            else
            {
                handler = UseInternalObjectResolver ?
                    TypeHelpers.ResolveObjectForType(type, messageContext.ServiceProvider.GetService, Id) :
                    messageContext.ServiceProvider.GetService(type);
            }

            // If we don't have handler - throw exception.
            if (handler == null)
            {
                messageContext.Status = ProcessingStatus.Rejected;
                throw new CommandHandlerNotFoundException(messageContext.Content.GetType().Name);
            }

            messageContext.Items[HandlerObjectKey] = handler;
        }

        /// <inheritdoc />
        public void PostHandle(IMessageContext messageContext)
        {
            if (UseInternalObjectResolver)
            {
                var handler = messageContext.GetItemByKey<object>(HandlerObjectKey);
                var disposable = handler as IDisposable;
                disposable?.Dispose();
            }
        }
    }
}
