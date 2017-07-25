// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Messages processing pipeline.
    /// </summary>
    public class MessagePipeline : IMessagePipeline, IDisposable
    {
        /// <summary>
        /// Middlewares list.
        /// </summary>
        public IMessagePipelineMiddleware[] Middlewares { get; set; }

        /// <summary>
        /// Options.
        /// </summary>
        public MessagePipelineOptions Options { get; set; } = new MessagePipelineOptions();

        #region IMessagePipeline

        /// <inheritdoc />
        public virtual byte[] MessageTypes => new byte[] { };

        /// <inheritdoc />
        public virtual void Invoke(IMessageContext messageContext)
        {
            for (int i = 0; i < Middlewares.Length; i++)
            {
                Middlewares[i].Handle(messageContext);
            }
            if (Options.ThrowExceptionOnFail && messageContext.FailException != null)
            {
                throw new MessageProcessingException("Processing exception.", messageContext.FailException);
            }
        }

        /// <summary>
        /// Processes the message thru all middlewares in async mode. Middleware should support
        /// <see cref="IAsyncMessagePipelineMiddleware" /> interface. Otherwise it will be called
        /// in sync mode.
        /// </summary>
        /// <param name="messageContext">The message context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public virtual async Task InvokeAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            // Execute message thru all middlewares.
            for (int i = 0; i < Middlewares.Length; i++)
            {
                var asyncHandler = Middlewares[i] as IAsyncMessagePipelineMiddleware;
                if (asyncHandler != null)
                {
                    await asyncHandler.HandleAsync(messageContext, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    Middlewares[i].Handle(messageContext);
                }
            }
            if (Options.ThrowExceptionOnFail && messageContext.FailException != null)
            {
                throw new MessageProcessingException("Processing exception.", messageContext.FailException);
            }
        }

        #endregion

        #region Dispose

        bool disposed;

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (var middleware in Middlewares)
                    {
                        var disposableMiddleware = middleware as IDisposable;
                        disposableMiddleware?.Dispose();
                    }
                    Middlewares = null;
                }
                disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return GetType().Name + ": " + string.Join(" >> ", Middlewares.Select(m => m.Id));
        }
    }
}
