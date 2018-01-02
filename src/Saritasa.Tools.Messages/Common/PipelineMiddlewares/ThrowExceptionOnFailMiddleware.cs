// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.ExceptionServices;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
    /// <summary>
    /// The middleware throws <see cref="MessageProcessingException" /> if message context has fail
    /// exception.
    /// </summary>
    public class ThrowExceptionOnFailMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = nameof(ThrowExceptionOnFailMiddleware);

        internal bool CheckExceptionDispatchInfo { get; set; } = false;

        /// <inheritdoc />
        public void Handle(IMessageContext messageContext)
        {
            if (CheckExceptionDispatchInfo)
            {
                if (messageContext.Items.TryGetValue(MessageContextConstants.ExceptionDispatchInfoKey, out object ediobj))
                {
                    var edi = ediobj as ExceptionDispatchInfo;
                    edi?.Throw();
                }
            }
            if (messageContext.FailException != null)
            {
                throw new MessageProcessingException("Processing exception.", messageContext.FailException);
            }
        }
    }
}
