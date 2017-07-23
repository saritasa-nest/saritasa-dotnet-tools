using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Queries;

namespace Saritasa.Tools.Messages.Queries
{
    /// <summary>
    /// The class wraps query object and makes actual call to query pipeline.
    /// </summary>
    /// <typeparam name="TQuery">Query type.</typeparam>
    public struct QueryCaller<TQuery> : IQueryCaller<TQuery> where TQuery : class
    {
        private TQuery query;

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
            var queryParameters = new QueryParameters
            {
                Parameters = args,
                QueryObject = query,
                FakeQueryObject = fakeQueryObject,
                Method = method
            };
            messageContext.Content = method.GetParameters().ToDictionary(p => p.Name, v => args[v.Position]);
            messageContext.ContentId = method.DeclaringType.FullName + "." + method.Name;
            messageContext.Items[QueryPipeline.QueryParametersKey] = queryParameters;
            if (invokeQuery)
            {
                queryPipeline.Invoke(messageContext);
                return (TResult)queryParameters.Result;
            }
            return default(TResult);
        }
    }
}
