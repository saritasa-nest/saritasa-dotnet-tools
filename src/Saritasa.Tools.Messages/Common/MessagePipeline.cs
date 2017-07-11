// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections.Generic;
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
        protected IList<IMessagePipelineMiddleware> Middlewares { get; set; } = new List<IMessagePipelineMiddleware>();

        #region IMessagePipeline

        /// <inheritdoc />
        public virtual byte[] MessageTypes => new byte[] { };

        /// <inheritdoc />
        public void AppendMiddlewares(params IMessagePipelineMiddleware[] middlewares)
        {
            foreach (var middleware in middlewares)
            {
                Middlewares.Add(middleware);
            }
            var ids = Middlewares.GroupBy(m => m.Id).Where(m => m.Count() > 1).ToArray();
            if (ids.Any())
            {
                Middlewares.Clear();
                throw new ArgumentException(string.Format(Properties.Strings.MiddlewareDuplicate, ids.First().Key));
            }
        }

        /// <inheritdoc />
        public IMessagePipelineMiddleware GetMiddlewareById(string id)
        {
            return Middlewares.FirstOrDefault(m => m.Id == id);
        }

        /// <inheritdoc />
        public void InsertMiddlewareAfter(IMessagePipelineMiddleware middleware, string insertAfterId = null)
        {
            if (middleware == null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }
            if (GetMiddlewareById(middleware.Id) != null)
            {
                throw new ArgumentException(string.Format(Properties.Strings.MiddlewareExists, middleware.Id));
            }

            if (string.IsNullOrEmpty(insertAfterId))
            {
                Middlewares.Add(middleware);
            }
            else
            {
                var insertAfterMiddleware = GetMiddlewareById(insertAfterId);
                if (insertAfterMiddleware == null)
                {
                    throw new MiddlewareNotFoundException();
                }
                Middlewares.Insert(Middlewares.IndexOf(insertAfterMiddleware) + 1, middleware);
            }
        }

        /// <inheritdoc />
        public void InsertMiddlewareBefore(IMessagePipelineMiddleware middleware, string insertBeforeId = null)
        {
            if (middleware == null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }
            if (GetMiddlewareById(middleware.Id) != null)
            {
                throw new ArgumentException(string.Format(Properties.Strings.MiddlewareExists, middleware.Id));
            }

            if (string.IsNullOrEmpty(insertBeforeId))
            {
                Middlewares.Insert(0, middleware);
            }
            else
            {
                var insertBeforeMiddleware = GetMiddlewareById(insertBeforeId);
                if (insertBeforeMiddleware == null)
                {
                    throw new MiddlewareNotFoundException();
                }
                Middlewares.Insert(Middlewares.IndexOf(insertBeforeMiddleware), middleware);
            }
        }

        /// <inheritdoc />
        public void RemoveMiddleware(string id)
        {
            var middleware = GetMiddlewareById(id);
            if (middleware == null)
            {
                throw new MiddlewareNotFoundException();
            }
            Middlewares.Remove(middleware);
        }

        /// <inheritdoc />
        public IEnumerable<IMessagePipelineMiddleware> GetMiddlewares()
        {
            return Middlewares;
        }

        /// <inheritdoc />
        public virtual void ProcessRaw(IMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Processes the message thru all middlewares.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void ProcessMiddlewares(IMessage message)
        {
            // Set execution context.
            MessageExecutionContext.Current = new MessageExecutionContext(message, this);

            // Execute message thru all middlewares.
            for (int i = 0; i < Middlewares.Count; i++)
            {
                Middlewares[i].Handle(message);
            }
        }

        /// <summary>
        /// Processes the message thru all middlewares in async mode. Middleware should support
        /// <see cref="IAsyncMessagePipelineMiddleware" /> interface. Otherwise it will be called
        /// in sync mode.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        protected async Task ProcessMiddlewaresAsync(IMessage message, CancellationToken cancellationToken)
        {
            // Set execution context.
            MessageExecutionContext.Current = new MessageExecutionContext(message, this);

            // Execute message thru all middlewares.
            for (int i = 0; i < Middlewares.Count; i++)
            {
                var asyncHandler = Middlewares[i] as IAsyncMessagePipelineMiddleware;
                if (asyncHandler != null)
                {
                    await asyncHandler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    Middlewares[i].Handle(message);
                }
            }
        }

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
                    Middlewares.Clear();
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
    }
}
