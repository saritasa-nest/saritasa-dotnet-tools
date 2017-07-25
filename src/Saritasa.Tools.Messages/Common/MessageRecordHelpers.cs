// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Message record helpers.
    /// </summary>
    public static class MessageRecordHelpers
    {
        /// <summary>
        /// Create message record from message context.
        /// </summary>
        /// <param name="messageContext">Message context.</param>
        /// <returns>Message record.</returns>
        public static MessageRecord Create(IMessageContext messageContext)
        {
            var messageRecord = new MessageRecord();
            object val;

            messageRecord.Id = messageContext.Id;
            if (messageContext.Items.TryGetValue(MessageContextConstants.TypeKey, out object type))
            {
                messageRecord.Type = (byte)type;
            }
            messageRecord.Content = messageContext.Content;
            messageRecord.ContentType = messageContext.ContentId;
            if (messageContext.Items.TryGetValue(MessageContextConstants.DataKey, out val))
            {
                messageRecord.Data = (IDictionary<string, string>)val;
            }
            messageRecord.Error = messageContext.FailException;
            messageRecord.ErrorType = TypeHelpers.GetPartiallyAssemblyQualifiedName(messageContext.FailException?.GetType());
            messageRecord.ErrorMessage = messageContext.FailException?.Message ?? string.Empty;
            if (messageContext.Items.TryGetValue(MessageContextConstants.ExecutionDurationKey, out val))
            {
                messageRecord.ExecutionDuration = (int)val;
            }
            return messageRecord;
        }
    }
}
