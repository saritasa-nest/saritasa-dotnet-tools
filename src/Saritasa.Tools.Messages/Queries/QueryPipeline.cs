// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using Saritasa.Tools.Messages.Internal;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Queries
{
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
#if NET452
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
                QueryObject = Activator.CreateInstance(method.GetBaseDefinition().DeclaringType, nonPublic: true),
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
                    query = (TQuery)Activator.CreateInstance(typeof(TQuery), nonPublic: true);
                    fakeQueryObject = true;
                }

                var mce = expression.Body as MethodCallExpression;
                if (mce == null)
                {
                    throw new InvalidOperationException(Properties.Strings.ExpressionMethodCallExpressionBody);
                }
                var args = mce.Arguments.Select(PartiallyEvaluateExpression).ToArray();
                var method = mce.Method;
                if (method.DeclaringType == null)
                {
                    throw new InvalidOperationException(Properties.Strings.MethodNoTypeDeclare);
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
                    var objectMember = Expression.Convert(expression, typeof(object));
                    var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                    var getter = getterLambda.Compile();
                    return getter();
                default:
                    throw new InvalidOperationException(
                        string.Format(Properties.Strings.CannotEvaluateExpression, expression.NodeType));
            }
        }

        /// <inheritdoc />
        public virtual ICaller<TQuery> Query<TQuery>() where TQuery : class
        {
            return new Caller<TQuery>(this);
        }

        /// <inheritdoc />
        public virtual ICaller<TQuery> Query<TQuery>(TQuery obj) where TQuery : class
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
                throw new ArgumentException(Properties.Strings.NoMethodNameFromContentType);
            }

            var objectTypeName = message.ContentType.Substring(0, message.ContentType.LastIndexOf(".", StringComparison.Ordinal));
#if NETSTANDARD1_5
            var objectType = TypeHelpers.LoadType(objectTypeName, TypeHelpers.LoadAssembliesFromTypeName(objectTypeName).ToArray());
#else
            var objectType = TypeHelpers.LoadType(objectTypeName, AppDomain.CurrentDomain.GetAssemblies());
#endif
            if (objectType == null)
            {
                throw new InvalidOperationException(string.Format(Properties.Strings.CannotLoadType, objectTypeName));
            }
            var obj = Activator.CreateInstance(objectType, nonPublic: true);

            var methodName = message.ContentType.Substring(message.ContentType.LastIndexOf(".", StringComparison.Ordinal) + 1);
            var method = objectType.GetTypeInfo().GetMethod(methodName);
            if (method == null)
            {
                throw new InvalidOperationException(string.Format(Properties.Strings.CannotFindMethod, methodName));
            }
            var delegateType = Expression.GetDelegateType(
                method.GetParameters().Select(p => p.ParameterType).Concat(new[] { method.ReturnType }).ToArray());
            var @delegate = obj.GetType().GetTypeInfo().GetMethod(methodName).CreateDelegate(delegateType, obj);
            if (@delegate == null)
            {
                throw new InvalidOperationException(Properties.Strings.CannotCreateDelegate);
            }

            var dictContent = message.Content as IDictionary<string, object>;
            if (dictContent == null)
            {
                throw new ArgumentException(string.Format(Properties.Strings.ContentShouldBeType,
                    nameof(IDictionary<string, object>)));
            }
            var messageContent = dictContent.Values;
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
    }
}
