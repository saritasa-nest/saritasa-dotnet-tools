// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Queries
{
    /// <summary>
    /// The class wraps query object and makes actual call to query pipeline.
    /// </summary>
    /// <typeparam name="TQuery">Query type.</typeparam>
    public struct QueryCaller<TQuery> : IQueryCaller<TQuery> where TQuery : class
    {
        private readonly TQuery query;

        private bool invokeQuery;

        private readonly QueryPipeline queryPipeline;

        private readonly IMessageContext messageContext;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="queryPipeline">Query pipeline.</param>
        /// <param name="messageContext">Message context.</param>
        public QueryCaller(QueryPipeline queryPipeline, IMessageContext messageContext)
        {
            this.query = null;
            this.queryPipeline = queryPipeline;
            this.messageContext = messageContext;
            this.invokeQuery = true;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="queryPipeline">Query pipeline.</param>
        /// <param name="messageContext">Message context.</param>
        /// <param name="query">Query object.</param>
        public QueryCaller(QueryPipeline queryPipeline, IMessageContext messageContext, TQuery query)
            : this(queryPipeline, messageContext)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            this.query = query;
        }

        /// <summary>
        /// Set no query execution. The default result will be returned in the case.
        /// </summary>
        /// <returns>Query caller.</returns>
        public QueryCaller<TQuery> NoExecution()
        {
            this.invokeQuery = false;
            return this;
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

        private QueryParameters PrepareQueryParametersAndMessageContext(MethodCallExpression mce)
        {
            var args = mce.Arguments.Select(PartiallyEvaluateExpression).ToArray();
            var method = mce.Method;
            if (method.DeclaringType == null)
            {
                throw new InvalidOperationException(Properties.Strings.MethodNoTypeDeclare);
            }

            var queryParameters = new QueryParameters
            {
                Parameters = args,
                QueryObject = query,
                Method = method
            };
            messageContext.Content = method.GetParameters().ToDictionary(p => p.Name, v => args[v.Position]);
            messageContext.ContentId = TypeHelpers.GetPartiallyAssemblyQualifiedName(method.DeclaringType);
            var indexOfComma = messageContext.ContentId.IndexOf(',');
            if (indexOfComma > -1)
            {
                messageContext.ContentId = messageContext.ContentId.Insert(indexOfComma, "." + method.Name);
            }
            messageContext.Items[QueryPipeline.QueryParametersKey] = queryParameters;
            return queryParameters;
        }

        /// <inheritdoc />
        public TResult With<TResult>(Expression<Func<TQuery, TResult>> expression)
        {
            var mce = expression.Body as MethodCallExpression;
            if (mce == null)
            {
                throw new InvalidOperationException(Properties.Strings.ExpressionMethodCallExpressionBody);
            }
            var queryParameters = PrepareQueryParametersAndMessageContext(mce);

            if (invokeQuery)
            {
                queryPipeline.Invoke(messageContext);
            }
            if (queryPipeline.Options.ThrowExceptionOnFail && messageContext.FailException != null)
            {
                throw new MessageProcessingException("Query processing error.", messageContext.FailException);
            }
            return (TResult)queryParameters.Result;
        }

        /// <inheritdoc />
        public async Task<TResult> WithAsync<TResult>(Expression<Func<TQuery, Task<TResult>>> expression,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var mce = expression.Body as MethodCallExpression;
            if (mce == null)
            {
                throw new InvalidOperationException(Properties.Strings.ExpressionMethodCallExpressionBody);
            }
            var queryParameters = PrepareQueryParametersAndMessageContext(mce);

            if (invokeQuery)
            {
                await queryPipeline.InvokeAsync(messageContext, cancellationToken);
            }
            if (queryPipeline.Options.ThrowExceptionOnFail && messageContext.FailException != null)
            {
                throw new MessageProcessingException("Query processing error.", messageContext.FailException);
            }
            return ((Task<TResult>)queryParameters.Result).Result;
        }
    }
}
