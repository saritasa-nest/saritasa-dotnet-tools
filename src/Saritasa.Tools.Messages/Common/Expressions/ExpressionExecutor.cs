using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using System;
using System.Linq.Expressions;
using System.Reflection;

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

        /// <summary>
        /// Ctor.
        /// </summary>
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
        /// Cache of compiled expressions.
        /// </summary>
        public ICompiledExpressionCache CompiledCache => compiledExpressionCache;

        /// <summary>
        /// Compiling expression or skipping if already compiled.
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
        /// Non generic execute.
        /// </summary>
        public object Execute(MethodInfo info, params object[] parameters)
        {
            var func = compiledExpressionCache.Get(info);

            return func.DynamicInvoke(parameters);
        }

        /// <summary>
        /// Execute on compiled delegate from cache.
        /// </summary>
        public TResult Execute<TInput, TResult>(MethodInfo info, TInput input)
        {
            var func = (Func<TInput, TResult>)compiledExpressionCache.Get(info);

            return func(input);
        }

        /// <summary>
        /// Generic execute of precompiled expression.
        /// </summary>
        public TResult Execute<TInput, TInput2, TResult>(MethodInfo info, TInput input, TInput2 input2)
        {
            var func = (Func<TInput, TInput2, TResult>)compiledExpressionCache.Get(info);

            return func(input, input2);
        }

        /// <summary>
        /// Generic execute of precompiled expression.
        /// </summary>
        public TResult Execute<TInput, TInput2, TInput3, TResult>(MethodInfo info, TInput input, TInput2 input2, TInput3 input3)
        {
            var func = (Func<TInput, TInput2, TInput3, TResult>)compiledExpressionCache.Get(info);

            return func(input, input2, input3);
        }

        /// <summary>
        /// Generic execute of precompiled expression.
        /// </summary>
        public TResult Execute<TInput, TInput2, TInput3, TInput4, TResult>(MethodInfo info, TInput input, TInput2 input2, TInput3 input3, TInput4 input4)
        {
            var func = (Func<TInput, TInput2, TInput3, TInput4, TResult>)compiledExpressionCache.Get(info);

            return func(input, input2, input3, input4);
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
