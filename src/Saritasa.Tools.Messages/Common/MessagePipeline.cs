// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Messages processing pipeline.
    /// </summary>
    public abstract class MessagePipeline : IMessagePipeline, IDisposable
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
                invokeDelegate = InternalInvokeInit;
                invokeDelegateAsync = InternalInvokeInitAsync;
            }
        }

        private Action<IMessageContext>[] middlewaresChain = new Action<IMessageContext>[0];

        private Func<IMessageContext, CancellationToken, Task>[] asyncMiddlewaresChain =
            new Func<IMessageContext, CancellationToken, Task>[0];

        private readonly object objLock = new object();

        #region IMessagePipeline

        /// <inheritdoc />
        public virtual byte[] MessageTypes => new byte[] { };

        private Action<MessagePipeline, IMessageContext> invokeDelegate = InternalInvokeInit;

        /// <inheritdoc />
        public virtual void Invoke(IMessageContext messageContext)
        {
            invokeDelegate(this, messageContext);
        }

        private static void InternalInvokeProcess(MessagePipeline messagePipeline, IMessageContext messageContext)
        {
            messageContext.Status = ProcessingStatus.Processing;
            for (int i = 0; i < messagePipeline.middlewaresChain.Length; i++)
            {
                messagePipeline.middlewaresChain[i](messageContext);
            }
        }

        private static void InternalInvokeInit(MessagePipeline messagePipeline, IMessageContext messageContext)
        {
            messagePipeline.middlewaresChain =
                messagePipeline.CreateSyncMiddlewaresChain(messagePipeline.middlewares).ToArray();
            messagePipeline.invokeDelegate = InternalInvokeProcess;
            messagePipeline.invokeDelegate(messagePipeline, messageContext);
        }

        private Func<MessagePipeline, IMessageContext, CancellationToken, Task> invokeDelegateAsync = InternalInvokeInitAsync;

        /// <summary>
        /// Processes the message thru all middlewares in async mode. Middleware should support
        /// <see cref="IAsyncMessagePipelineMiddleware" /> interface. Otherwise it will be called
        /// in sync mode.
        /// </summary>
        /// <param name="messageContext">The message context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public virtual Task InvokeAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            return invokeDelegateAsync(this, messageContext, cancellationToken);
        }

        private static async Task InternalInvokeProcessAsync(MessagePipeline messagePipeline, IMessageContext messageContext,
            CancellationToken cancellationToken)
        {
            // Execute message thru all middlewares.
            messageContext.Status = ProcessingStatus.Processing;
            for (int i = 0; i < messagePipeline.asyncMiddlewaresChain.Length; i++)
            {
                await messagePipeline.asyncMiddlewaresChain[i](messageContext, cancellationToken);
            }
        }

        private static Task InternalInvokeInitAsync(MessagePipeline messagePipeline, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            messagePipeline.asyncMiddlewaresChain =
                messagePipeline.CreateAsyncMiddlewaresChain(messagePipeline.middlewares).ToArray();
            messagePipeline.invokeDelegateAsync = InternalInvokeProcessAsync;
            return messagePipeline.invokeDelegateAsync(messagePipeline, messageContext, cancellationToken);
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
