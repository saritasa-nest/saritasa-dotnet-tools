// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Message data transfer object used by repositories to save.
    /// </summary>
    public class MessageRecord
    {
        /// <summary>
        /// Unique message id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Message type.
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// Message type name.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Message serialized content. May be command object, or event object.
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// Custom data. Should be <see cref="IDictionary{TKey,TValue}"/> where
        /// TKey and TValue are a <see cref="string" /> type.
        /// </summary>
        public IDictionary<string, string> Data { get; set; }

        /// <summary>
        /// Contains exception if any error occured during message processing.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Error text message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Error type.
        /// </summary>
        public string ErrorType { get; set; }

        /// <summary>
        /// When message has been created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Message execution duration, in ms.
        /// </summary>
        public int ExecutionDuration { get; set; }

        /// <summary>
        /// Processing status.
        /// </summary>
        public ProcessingStatus Status { get; set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public MessageRecord()
        {
        }

        /// <summary>
        /// .ctor to create from message context. It allows partially prepare record object.
        /// </summary>
        /// <param name="messageContext">Message context.</param>
        public MessageRecord(IMessageContext messageContext)
        {
            object val;

            Id = messageContext.Id;
            if (messageContext.Items.TryGetValue(MessageContextConstants.TypeKey, out object type))
            {
                Type = (byte)type;
            }
            Content = messageContext.Content;
            ContentType = messageContext.ContentId;
            if (messageContext.Items.TryGetValue(MessageContextConstants.DataKey, out val))
            {
                Data = (IDictionary<string, string>)val;
            }
            Error = messageContext.FailException;
            ErrorType = TypeHelpers.GetPartiallyAssemblyQualifiedName(messageContext.FailException?.GetType());
            ErrorMessage = messageContext.FailException?.Message ?? string.Empty;
            if (messageContext.Items.TryGetValue(MessageContextConstants.ExecutionDurationKey, out val))
            {
                ExecutionDuration = (int)val;
            }
        }
    }
}
