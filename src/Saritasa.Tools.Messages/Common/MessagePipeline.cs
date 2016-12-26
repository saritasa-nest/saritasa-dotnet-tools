// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Abstractions;

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
                throw new ArgumentException($"There are middlewares with duplicated identifiers: {ids.First().Key}");
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
                throw new ArgumentException($"Middleware with id {middleware.Id} already exists");
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
                throw new ArgumentException($"Middleware with id {middleware.Id} already exists");
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
            // set execution context
            MessageExecutionContext.Current = new MessageExecutionContext(message, this);

            // execute message thru all middlewares
            foreach (var handler in Middlewares)
            {
                handler.Handle(message);
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
