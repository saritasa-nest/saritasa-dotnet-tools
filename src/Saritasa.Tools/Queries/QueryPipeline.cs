// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Messages;
    using System.Reflection;

    /// <summary>
    /// Query pipeline.
    /// </summary>
    public class QueryPipeline : MessagePipeline, IQueryPipeline
    {
        private QueryMessage CreateMessage(Delegate func, params object[] args)
        {
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
            var method = func.Method;
#else
            var method = func.GetInvocationList()[0].GetMethodInfo();
#endif

            return new QueryMessage()
            {
                ContentType = method.DeclaringType.FullName + "." + method.Name,
                Content = method.GetParameters().ToDictionary(p => p.Name, v => args[v.Position]),
                CreatedAt = DateTime.Now,
                Status = Message.ProcessingStatus.Processing,
                Parameters = args,
                Func = func,
            };
        }

        private void ProcessPipeline(QueryMessage message)
        {
            foreach (var handler in Middlewares)
            {
                handler.Handle(message);
            }
            if (message.ErrorDispatchInfo != null)
            {
                message.ErrorDispatchInfo.Throw();
            }
        }

        /// <inheritdoc />
        public TResult Execute<TResult>(Func<TResult> func)
        {
            var message = CreateMessage(func, new Dictionary<string, object>());
            ProcessPipeline(message);
            return (TResult)message.Result;
        }

        /// <inheritdoc />
        public TResult Execute<T, TResult>(Func<T, TResult> func, T arg)
        {
            var message = CreateMessage(func, arg);
            ProcessPipeline(message);
            return (TResult)message.Result;
        }

        /// <inheritdoc />
        public TResult Execute<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 arg1, T2 arg2)
        {
            var message = CreateMessage(func, arg1, arg2);
            ProcessPipeline(message);
            return (TResult)message.Result;
        }

        /// <inheritdoc />
        public TResult Execute<T1, T2, T3, TResult>(Func<T1, T2, TResult> func, T1 arg1, T2 arg2, T3 arg3)
        {
            var message = CreateMessage(func, arg1, arg2, arg3);
            ProcessPipeline(message);
            return (TResult)message.Result;
        }

        /// <summary>
        /// Creates default pipeline with query executor.
        /// </summary>
        /// <returns>Query pipeline.</returns>
        public static QueryPipeline CreateDefaultPipeline()
        {
            var queryPipeline = new QueryPipeline();
            queryPipeline.AddMiddlewares(new QueryPipelineMIddlewares.QueryExecutorMiddleware());
            return queryPipeline;
        }
    }
}
