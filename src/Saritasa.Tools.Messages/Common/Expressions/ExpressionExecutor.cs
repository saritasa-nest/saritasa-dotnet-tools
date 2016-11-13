using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    /// <summary>
    /// Main aggregate object for use.
    /// </summary>
    public class ExpressionExecutor
    {
        private ICompiledExpressionCache compiledExpressionCache;
        private IExpressionCompilator expressionCompilator;
        private IExpressionTransformVisitorFactory transformVisitorFactory;

        public ExpressionExecutor(
            ICompiledExpressionCache expressionProvider,
            IExpressionCompilator expressionCompilator,
            IExpressionTransformVisitorFactory transformVisitorFactory)
        {
            this.expressionCompilator = expressionCompilator;
            this.compiledExpressionCache = expressionProvider;
            this.transformVisitorFactory = transformVisitorFactory;
        }

        /// <summary>
        /// Count of cached items.
        /// </summary>
        public int CacheCount => compiledExpressionCache.Count;

        /// <summary>
        /// Clearing cache.
        /// </summary>
        public void ClearCache() => compiledExpressionCache.Clear();

        /// <summary>
        /// Compiling expression or returning already compiled.
        /// </summary>
        public void PreCompile(Expression expression)
        {
            compiledExpressionCache.GetOrAdd(GetMethodInfo(expression), () =>
            {
                var transformer = transformVisitorFactory.Create();
                var transformedExpression = transformer.Visit(expression);
                return expressionCompilator.Compile(transformedExpression);
            });
        }

        /// <summary>
        /// Execute on compiled delegate from cache.
        /// </summary>
        public TResult Execute<TInput, TResult>(MethodInfo info, TInput input)
        {
            var func = (Func<TInput, TResult>)compiledExpressionCache.Get(info);

            return func(input);
        }

        public TResult Execute<TInput, TInput2, TResult>(MethodInfo info, TInput input, TInput2 input2)
        {
            var func = (Func<TInput, TInput2, TResult>)compiledExpressionCache.Get(info);

            return func(input, input2);
        }

        public TResult Execute<TInput, TInput2, TInput3, TResult>(MethodInfo info, TInput input, TInput2 input2, TInput3 input3)
        {
            var func = (Func<TInput, TInput2, TInput3, TResult>)compiledExpressionCache.Get(info);

            return func(input, input2, input3);
        }

        private MethodInfo GetMethodInfo(Expression expression)
        {
            var lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression != null)
            {
                var body = lambdaExpression.Body;
                if (body is MethodCallExpression)
                {
                    return (body as MethodCallExpression).Method;
                }
            }

            throw new NotSupportedException();
        }
    }
}
