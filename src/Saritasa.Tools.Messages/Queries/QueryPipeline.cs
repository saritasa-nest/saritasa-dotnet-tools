// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Linq;
    using System.Reflection;
    using Internal;
    using Abstractions;
    using Common;

    /// <summary>
    /// Query pipeline.
    /// </summary>
    public class QueryPipeline : MessagePipeline, IQueryPipeline
    {
        static readonly byte[] availableMessageTypes = { Message.MessageTypeQuery };

        /// <inheritdoc />
        public override byte[] MessageTypes => availableMessageTypes;

        static QueryMessage CreateMessage(Delegate func, params object[] args)
        {
#if !NETCOREAPP1_1 && !NETSTANDARD1_6
            var method = func.Method;
#else
            var method = func.GetInvocationList()[0].GetMethodInfo();
#endif

            return new QueryMessage()
            {
                ContentType = method.DeclaringType.FullName + "." + method.Name,
                Content = method.GetParameters().ToDictionary(p => p.Name, v => args[v.Position]),
                CreatedAt = DateTime.Now,
                Status = ProcessingStatus.Processing,
                Parameters = args,
                Method = method,
                QueryObject = CreateObjectFromType(method.GetBaseDefinition().DeclaringType),
                FakeQueryObject = true,
            };
        }

        void ProcessPipeline(QueryMessage message)
        {
            ProcessMiddlewares(message);
            message.ErrorDispatchInfo?.Throw();
        }

        /// <summary>
        /// The class wraps query object and makes actual call to query pipeline.
        /// </summary>
        /// <typeparam name="TQuery">Query type.</typeparam>
        public struct Caller<TQuery> : ICaller<TQuery> where TQuery : class
        {
            TQuery query;

            readonly QueryPipeline queryPipeline;

            /// <summary>
            /// .ctor
            /// </summary>
            /// <param name="queryPipeline">Query pipeline.</param>
            public Caller(QueryPipeline queryPipeline)
            {
                this.query = null;
                this.queryPipeline = queryPipeline;
            }

            /// <summary>
            /// .ctor
            /// </summary>
            /// <param name="queryPipeline">Query pipeline.</param>
            /// <param name="query">Query object.</param>
            public Caller(QueryPipeline queryPipeline, TQuery query) : this(queryPipeline)
            {
                if (query == null)
                {
                    throw new ArgumentNullException(nameof(query));
                }
                this.query = query;
            }

            /// <inheritdoc />
            public TResult With<TResult>(Expression<Func<TQuery, TResult>> expression)
            {
                bool fakeQueryObject = false;
                if (query == null)
                {
                    query = (TQuery)CreateObjectFromType(typeof(TQuery));
                    fakeQueryObject = true;
                }

                var mce = expression.Body as MethodCallExpression;
                if (mce == null)
                {
                    throw new InvalidOperationException("Expression must have Body of type MethodCallExpression");
                }
                var args = mce.Arguments.Select(PartiallyEvaluateExpression).ToArray();
                var method = mce.Method;
                if (method.DeclaringType == null)
                {
                    throw new InvalidOperationException("Method does not declare type");
                }
                var message = new QueryMessage()
                {
                    ContentType = method.DeclaringType.FullName + "." + method.Name,
                    Content = method.GetParameters().ToDictionary(p => p.Name, v => args[v.Position]),
                    CreatedAt = DateTime.Now,
                    Status = ProcessingStatus.Processing,
                    Parameters = args,
                    QueryObject = query,
                    FakeQueryObject = fakeQueryObject,
                    Method = method,
                };
                queryPipeline.ProcessPipeline(message);
                return (TResult)message.Result;
            }
        }

        private static object PartiallyEvaluateExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return ((ConstantExpression)expression).Value;
                case ExpressionType.MemberAccess:
                    // TODO: carefully check for performance
                    var objectMember = Expression.Convert(expression, typeof(object));
                    var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                    var getter = getterLambda.Compile();
                    return getter();
                default:
                    throw new InvalidOperationException($"Cannot evaluate type {expression.NodeType}");
            }
        }

        /// <inheritdoc />
        public ICaller<TQuery> Query<TQuery>() where TQuery : class
        {
            return new Caller<TQuery>(this);
        }

        /// <inheritdoc />
        public ICaller<TQuery> Query<TQuery>(TQuery obj) where TQuery : class
        {
            return new Caller<TQuery>(this, obj);
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
            queryPipeline.AppendMiddlewares(new PipelineMiddlewares.QueryObjectResolverMiddleware(resolver));
            queryPipeline.AppendMiddlewares(new PipelineMiddlewares.QueryExecutorMiddleware());
            queryPipeline.AppendMiddlewares(new PipelineMiddlewares.QueryObjectReleaseMiddleware());
            return queryPipeline;
        }

        /// <inheritdoc cref="IMessagePipeline.ProcessRaw" />
        public override void ProcessRaw(IMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (string.IsNullOrEmpty(message.ContentType))
            {
                throw new ArgumentException(nameof(message.ContentType));
            }
            if (message.ContentType.IndexOf(".", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Cannot specify method name and type from content type");
            }

            var objectTypeName = message.ContentType.Substring(0, message.ContentType.LastIndexOf(".", StringComparison.Ordinal));
#if NETCOREAPP1_1 || NETSTANDARD1_6
            var objectType = TypeHelpers.LoadType(objectTypeName, TypeHelpers.LoadAssembliesFromTypeName(objectTypeName).ToArray());
#else
            var objectType = TypeHelpers.LoadType(objectTypeName, AppDomain.CurrentDomain.GetAssemblies());
#endif
            if (objectType == null)
            {
                throw new InvalidOperationException($"Cannot load type {objectTypeName}");
            }
            var obj = CreateObjectFromType(objectType);

            var methodName = message.ContentType.Substring(message.ContentType.LastIndexOf(".", StringComparison.Ordinal) + 1);
            var method = objectType.GetTypeInfo().GetMethod(methodName);
            if (method == null)
            {
                throw new InvalidOperationException($"Cannot find method {methodName}");
            }
            var delegateType = Expression.GetDelegateType(
                method.GetParameters().Select(p => p.ParameterType).Concat(new[] { method.ReturnType }).ToArray());
            var @delegate = obj.GetType().GetTypeInfo().GetMethod(methodName).CreateDelegate(delegateType, obj);
            if (@delegate == null)
            {
                throw new Exception("Cannot create delegate");
            }

            var messageContent = ((IDictionary<string, object>)message.Content).Values;
            var methodTypes = method.GetParameters().Select(p => p.ParameterType);
            var values = methodTypes.Zip(messageContent, (mt, mc) => TypeHelpers.ConvertType(mc, mt));

            var queryMessage = CreateMessage(@delegate, values.ToArray());
            ProcessPipeline(queryMessage);
            message.Content = queryMessage.Result;
            message.Error = queryMessage.Error;
            message.ErrorMessage = queryMessage.ErrorMessage;
            message.ErrorType = queryMessage.ErrorType;
            message.Status = queryMessage.Status;
            message.ExecutionDuration = queryMessage.ExecutionDuration;
            message.Data = queryMessage.Data;
        }

        /// <summary>
        /// Create object from type. Type must have parameterless ctor.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Created object.</returns>
        private static object CreateObjectFromType(Type t)
        {
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
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
