using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Saritasa.Tools.Messages.Common.Expressions.Compilation;

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
        private static ConcurrentDictionary<MethodInfo, MethodInfo> genericInvocationCache =
            new ConcurrentDictionary<MethodInfo, MethodInfo>();

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
        /// Dynamic execute for compiled expressions.
        /// </summary>
        /// <param name="info">info, used as key.</param>
        /// <param name="parameters">parameters for func.</param>
        public dynamic Execute(MethodInfo info, params dynamic[] parameters)
        {
            dynamic func = compiledExpressionCache.Get(info);

            if (parameters.Count() == 0)
            {
                return func.Invoke();
            }
            if (parameters.Count() == 1)
            {
                return func.Invoke(parameters[0]);
            }
            else if (parameters.Count() == 2)
            {
                return func.Invoke(parameters[0], parameters[1]);
            }
            else if (parameters.Count() == 3)
            {
                return func.Invoke(parameters[0], parameters[1], parameters[2]);
            }
            else if (parameters.Count() == 4)
            {
                return func.Invoke(parameters[0], parameters[1], parameters[2], parameters[3]);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Execute on compiled delegate from cache.
        /// </summary>
        public TResult ExecuteTyped<TInput, TResult>(MethodInfo info, TInput input)
        {
            var func = (Func<TInput, TResult>)compiledExpressionCache.Get(info);

            return func(input);
        }

        /// <summary>
        /// Generic execute of precompiled expression.
        /// </summary>
        public TResult ExecuteTyped<TInput, TInput2, TResult>(MethodInfo info, TInput input, TInput2 input2)
        {
            var func = (Func<TInput, TInput2, TResult>)compiledExpressionCache.Get(info);

            return func(input, input2);
        }

        /// <summary>
        /// Generic execute of precompiled expression.
        /// </summary>
        public TResult ExecuteTyped<TInput, TInput2, TInput3, TResult>(MethodInfo info, TInput input, TInput2 input2, TInput3 input3)
        {
            var func = (Func<TInput, TInput2, TInput3, TResult>)compiledExpressionCache.Get(info);

            return func(input, input2, input3);
        }

        /// <summary>
        /// Generic execute of precompiled expression.
        /// </summary>
        public TResult ExecuteTyped<TInput, TInput2, TInput3, TInput4, TResult>(MethodInfo info, TInput input, TInput2 input2, TInput3 input3, TInput4 input4)
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
