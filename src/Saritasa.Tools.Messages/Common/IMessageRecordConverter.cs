// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// The implementer should provide method to convert to/from
    /// <see cref="MessageRecord" /> and <see cref="IMessageContext" />.
    /// </summary>
    public interface IMessageRecordConverter
    {
        /// <summary>
        /// Convert <see cref="MessageRecord" /> to <see cref="IMessageContext" />.
        /// </summary>
        /// <param name="pipelineService">Pipeline service to be used to create message context.</param>
        /// <param name="record">Message record to covert from.</param>
        /// <returns>Message context to convert to.</returns>
        IMessageContext CreateMessageContext(IMessagePipelineService pipelineService, MessageRecord record);

        /// <summary>
        /// Create message record from <see cref="IMessageContext" />.
        /// </summary>
        /// <param name="context">Message context.</param>
        /// <returns>Message record.</returns>
        MessageRecord CreateMessageRecord(IMessageContext context);
    }
}
