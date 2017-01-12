﻿// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Queries
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Messages;

    /// <summary>
    /// Query pipeline.
    /// </summary>
    public class QueryPipeline : MessagePipeline, IQueryPipeline
    {
        static readonly byte[] AvailableMessageTypes = new byte[] { Message.MessageTypeQuery };

        /// <inheritdoc />
        public override byte[] MessageTypes => AvailableMessageTypes;

        static QueryMessage CreateMessage(Delegate func, params object[] args)
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

        void ProcessPipeline(QueryMessage message)
        {
            foreach (var handler in Middlewares)
            {
                handler.Handle(message);
            }
            message.ErrorDispatchInfo?.Throw();
        }

        /// <inheritdoc />
        public TResult Execute<TResult>(Func<TResult> func)
        {
            var message = CreateMessage(func);
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
        public TResult Execute<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 arg1, T2 arg2, T3 arg3)
        {
            var message = CreateMessage(func, arg1, arg2, arg3);
            ProcessPipeline(message);
            return (TResult)message.Result;
        }

        /// <inheritdoc />
        public TQuery GetQuery<TQuery>() where TQuery : class
        {
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
            var ctor = typeof(TQuery).GetTypeInfo().GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new Type[] { }, null);
#else
            var ctorMember = typeof(TQuery).GetTypeInfo().FindMembers(
                MemberTypes.Constructor,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                (member, filterCriteria) => true,
                null).FirstOrDefault();
            var ctor = ctorMember != null ? (ConstructorInfo) ctorMember : (ConstructorInfo)null;
#endif
            if (ctor == null)
            {
                throw new ArgumentException($"Type {typeof(TQuery)} must have public or private parameter-less constructor");
            }

            return (TQuery)ctor.Invoke(new object[] { });
        }

        /// <summary>
        /// Resolver that always returns null values.
        /// </summary>
        /// <param name="type">Type to resolve.</param>
        /// <returns>Null.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "type",
            Justification = "Mock method")]
        public static object NullResolver(Type type)
        {
            return null;
        }

        /// <summary>
        /// Creates default pipeline with query executor.
        /// </summary>
        /// <returns>Query pipeline.</returns>
        public static QueryPipeline CreateDefaultPipeline(Func<Type, object> resolver)
        {
            var queryPipeline = new QueryPipeline();
            queryPipeline.AppendMiddlewares(new QueryPipelineMIddlewares.QueryObjectResolverMiddleware(resolver));
            queryPipeline.AppendMiddlewares(new QueryPipelineMIddlewares.QueryExecutorMiddleware());
            return queryPipeline;
        }

        /// <inheritdoc />
        public override void ProcessRaw(Message message)
        {
            throw new NotImplementedException();
        }
    }
}