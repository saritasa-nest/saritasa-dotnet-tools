// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using Saritasa.Tools.Internal;
using Saritasa.Tools.Messages;

namespace Saritasa.Tools.Queries
{
    /// <summary>
    /// Query pipeline.
    /// </summary>
    public class QueryPipeline : MessagePipeline, IQueryPipeline
    {
        static readonly byte[] AvailableMessageTypes = { Message.MessageTypeQuery };

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
        public TResult Execute<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var message = CreateMessage(func, arg1, arg2, arg3, arg4);
            ProcessPipeline(message);
            return (TResult)message.Result;
        }

        /// <inheritdoc />
        public TResult Execute<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var message = CreateMessage(func, arg1, arg2, arg3, arg4, arg5);
            ProcessPipeline(message);
            return (TResult)message.Result;
        }

        /// <inheritdoc />
        public TResult Execute<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var message = CreateMessage(func, arg1, arg2, arg3, arg4, arg5, arg6);
            ProcessPipeline(message);
            return (TResult)message.Result;
        }

        /// <inheritdoc />
        public TResult Execute<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            var message = CreateMessage(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            ProcessPipeline(message);
            return (TResult)message.Result;
        }

        /// <inheritdoc />
        public TResult Execute<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            var message = CreateMessage(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            ProcessPipeline(message);
            return (TResult)message.Result;
        }

        /// <inheritdoc />
        public TQuery GetQuery<TQuery>() where TQuery : class
        {
            return (TQuery)CreateObjectFromType(typeof(TQuery));
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
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (string.IsNullOrEmpty(message.ContentType))
            {
                throw new ArgumentException(nameof(message.ContentType));
            }
            if (message.ContentType.IndexOf(".") < 0)
            {
                throw new ArgumentException("Cannot specify method name and type from content type");
            }

            var objectTypeName = message.ContentType.Substring(0, message.ContentType.LastIndexOf(".", StringComparison.Ordinal));
            var objectType = TypeHelpers.LoadType(objectTypeName, AppDomain.CurrentDomain.GetAssemblies());
            var obj = CreateObjectFromType(objectType);

            var methodName = message.ContentType.Substring(message.ContentType.LastIndexOf(".", StringComparison.Ordinal) + 1);
            var method = objectType.GetMethod(methodName);
            if (method == null)
            {
                throw new ArgumentException($"Cannot find method {methodName}");
            }
            var delegateType = Expression.GetDelegateType(
                method.GetParameters().Select(p => p.ParameterType).Concat(new[] { method.ReturnType }).ToArray());
            var @delegate = obj.GetType().GetMethod(methodName).CreateDelegate(delegateType, obj);
            if (@delegate == null)
            {
                throw new Exception("Cannot create delegate");
            }

            var messageContent = ((IDictionary<string, object>)message.Content).Values;
            var methodTypes = method.GetParameters().Select(p => p.ParameterType);
            var values = methodTypes.Zip(messageContent, (mt, mc) =>
            {
                return TypeHelpers.ConvertType(mc, mt);
            });

            var queryMessage = CreateMessage(@delegate, values.ToArray());
            ProcessPipeline(queryMessage);
            message.Content = queryMessage.Result;
#endif
        }

        /// <summary>
        /// Create object from type. Type must have parameterless ctor.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Created object.</returns>
        private object CreateObjectFromType(Type t)
        {
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
            var ctor = t.GetTypeInfo().GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new Type[] { }, null);
#else
            var ctorMember = t.GetTypeInfo().FindMembers(
                MemberTypes.Constructor,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                (member, filterCriteria) => true,
                null).FirstOrDefault();
            var ctor = ctorMember != null ? (ConstructorInfo) ctorMember : (ConstructorInfo)null;
#endif
            if (ctor == null)
            {
                throw new ArgumentException($"Type {t} must have public or private parameter-less constructor");
            }

            return ctor.Invoke(new object[] { });
        }
    }
}
