// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common.PipelineMiddlewares;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Messages processing pipeline.
    /// </summary>
    public class MessagePipeline : IMessagePipeline, IDisposable
    {
        private IMessagePipelineMiddleware[] middlewares;

        /// <summary>
        /// Middlewares list.
        /// </summary>
        public IMessagePipelineMiddleware[] Middlewares
        {
            get => middlewares;
            set
            {
                middlewares = value;
                InitializeMiddlewaresChains();
            }
        }

        /// <summary>
        /// Options.
        /// </summary>
        public MessagePipelineOptions Options { get; set; } = new MessagePipelineOptions();

        private Action<IMessageContext>[] middlewaresChain;

        private Func<IMessageContext, CancellationToken, Task>[] asyncMiddlewaresChain;

        private readonly object objLock = new object();

        #region IMessagePipeline

        /// <inheritdoc />
        public virtual byte[] MessageTypes => new byte[] { };

        /// <inheritdoc />
        public virtual void Invoke(IMessageContext messageContext)
        {
            var localMiddlewaresChain = middlewaresChain;
            messageContext.Status = ProcessingStatus.Processing;
            for (int i = 0; i < localMiddlewaresChain.Length; i++)
            {
                localMiddlewaresChain[i](messageContext);
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
            var localAsyncMiddlewaresChain = asyncMiddlewaresChain;
            messageContext.Status = ProcessingStatus.Processing;
            for (int i = 0; i < localAsyncMiddlewaresChain.Length; i++)
            {
                await localAsyncMiddlewaresChain[i](messageContext, cancellationToken);
            }
        }

        #endregion

        private Func<IMessageContext, CancellationToken, Task> CreateAsyncCallWrapper(Action<IMessageContext> action)
        {
            Task Func(IMessageContext mc, CancellationToken ct)
            {
                action(mc);
                return Task.FromResult(0);
            }

            return Func;
        }

        private void InitializeMiddlewaresChains()
        {
            lock (objLock)
            {
                var localMiddlewares = middlewares;
                middlewaresChain = CreateSyncMiddlewaresChain(localMiddlewares).ToArray();
                asyncMiddlewaresChain = CreateAsyncMiddlewaresChain(localMiddlewares).ToArray();
            }
        }

        private List<Action<IMessageContext>> CreateSyncMiddlewaresChain(IMessagePipelineMiddleware[] localMiddlewares)
        {
            var list = new List<Action<IMessageContext>>();
            for (int i = 0; i < localMiddlewares.Length; i++)
            {
                list.Add(localMiddlewares[i].Handle);
            }
            for (int i = localMiddlewares.Length - 1; i >= 0; i--)
            {
                var postActionMiddleware = localMiddlewares[i] as IMessagePipelinePostAction;
                if (postActionMiddleware != null)
                {
                    list.Add(postActionMiddleware.PostHandle);
                }
            }
            if (Options.ThrowExceptionOnFail)
            {
                var throwExceptionMiddleware = new ThrowExceptionOnFailMiddleware();
                list.Add(throwExceptionMiddleware.Handle);
            }
            return list;
        }

        private List<Func<IMessageContext, CancellationToken, Task>> CreateAsyncMiddlewaresChain(
            IMessagePipelineMiddleware[] localMiddlewares)
        {
            var list = new List<Func<IMessageContext, CancellationToken, Task>>();
            for (int i = 0; i < localMiddlewares.Length; i++)
            {
                var asyncMiddleware = localMiddlewares[i] as IAsyncMessagePipelineMiddleware;
                if (asyncMiddleware != null)
                {
                    list.Add(asyncMiddleware.HandleAsync);
                }
                else
                {
                    list.Add(CreateAsyncCallWrapper(localMiddlewares[i].Handle));
                }
            }
            for (int i = localMiddlewares.Length - 1; i >= 0; i--)
            {
                var postActionMiddleware = localMiddlewares[i] as IMessagePipelinePostAction;
                if (postActionMiddleware != null)
                {
                    list.Add(CreateAsyncCallWrapper(postActionMiddleware.PostHandle));
                }
            }
            if (Options.ThrowExceptionOnFail)
            {
                var throwExceptionMiddleware = new ThrowExceptionOnFailMiddleware();
                list.Add(CreateAsyncCallWrapper(throwExceptionMiddleware.Handle));
            }
            return list;
        }

        #region Dispose

        bool disposed;

        /// <summary>
        /// Releases middleware resources.
        /// </summary>
        /// <param name="disposing">Is called from Dispose method.</param>
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
