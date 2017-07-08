// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET452 || NET40
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#else
using System.Threading;
#endif
using JetBrains.Annotations;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Message execution context represent current running message on pipeline. It stores message
    /// and message pipeline in "async TLS" and this info available also after message processing.
    /// </summary>
    public struct MessageExecutionContext
    {
        /// <summary>
        /// Message.
        /// </summary>
        public IMessage Message { get; }

        /// <summary>
        /// Message pipeline.
        /// </summary>
        public IMessagePipeline MessagePipeline { get; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="message">Current processing message.</param>
        /// <param name="messagePipeline">Message pipeline that processes the mesage.</param>
        public MessageExecutionContext(
            [NotNull] IMessage message,
            [NotNull] IMessagePipeline messagePipeline)
        {
            Message = message;
            MessagePipeline = messagePipeline;
        }

        /// <summary>
        /// Represents empty <inheritdoc cref="MessageExecutionContext" /> structure.
        /// </summary>
        public static MessageExecutionContext Empty = default(MessageExecutionContext);

#if NET452 || NET40
        private static readonly string fieldKey = $"{typeof(MessageExecutionContext).FullName}.{AppDomain.CurrentDomain.Id}";

        /// <summary>
        /// Current message execution scope related to current execution (thread) context.
        /// </summary>
        public static MessageExecutionContext Current
        {
            get
            {
                var handle = CallContext.LogicalGetData(fieldKey) as ObjectHandle;
                return (MessageExecutionContext) handle?.Unwrap();
            }

            set
            {
                CallContext.LogicalSetData(fieldKey, new ObjectHandle(value));
            }
        }

        /// <summary>
        /// Is messages context initialized.
        /// </summary>
        public static bool IsInitialized => CallContext.LogicalGetData(fieldKey) != null;
#else
        private static readonly AsyncLocal<MessageExecutionContext?> localValue = new AsyncLocal<MessageExecutionContext?>();

        /// <summary>
        /// Current message execution scope related to current execution (thread) context.
        /// </summary>
        public static MessageExecutionContext Current
        {
            set
            {
                localValue.Value = value;
            }

            get
            {
                return localValue.Value ?? Empty;
            }
        }

        /// <summary>
        /// Is messages context initialized.
        /// </summary>
        public static bool IsInitialized => localValue.Value.HasValue;
#endif
    }
}
