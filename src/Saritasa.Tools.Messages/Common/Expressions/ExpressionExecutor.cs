using System;
using System.Collections.Generic;
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
        private IExpressionReduceVisitorFactory reduceVisitorFactory;

        private Dictionary<int, Func<dynamic, dynamic[], dynamic>> callDispatchers = new Dictionary<int, Func<dynamic, dynamic[], dynamic>>
        {
            [0] = (func, parameters) => func.Invoke(),
            [1] = (func, parameters) => func.Invoke(parameters[0]),
            [2] = (func, parameters) => func.Invoke(parameters[0], parameters[1]),
            [3] = (func, parameters) => func.Invoke(parameters[0], parameters[1], parameters[2]),
            [4] = (func, parameters) => func.Invoke(parameters[0], parameters[1], parameters[2], parameters[3]),
            [5] = (func, parameters) => func.Invoke(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]),
            [6] = (func, parameters) => func.Invoke(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]),
            [7] = (func, parameters) => func.Invoke(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]),
            [8] = (func, parameters) => func.Invoke(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7]),
            [9] = (func, parameters) => func.Invoke(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8]),
            [10] = (func, parameters) => func.Invoke(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9]),
            [11] = (func, parameters) => func.Invoke(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10]),
            [12] = (func, parameters) => func.Invoke(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11])
        };

        /// <summary>
        /// Ctor.
        /// </summary>
        public ExpressionExecutor(
            ICompiledExpressionCache expressionProvider,
            IExpressionCompilator expressionCompilator,
            IExpressionTransformVisitorFactory transformVisitorFactory,
            IExpressionReduceVisitorFactory reduceVisitorFactory)
        {
            this.expressionCompilator = expressionCompilator;
            this.compiledExpressionCache = expressionProvider;
            this.transformVisitorFactory = transformVisitorFactory;
            this.reduceVisitorFactory = reduceVisitorFactory;
        }

        /// <summary>
        /// Cache of compiled expressions.
        /// </summary>
        public ICompiledExpressionCache CompiledCache => compiledExpressionCache;

        /// <summary>
        /// Compiling expression or skipping if already compiled.
        /// If expression need transformation, it will be transformed.
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
        /// Reducing expression for collapse binary operations in query like +, -, /, *, &lt;, &gt;, &lt;&lt;, &gt;&gt;.
        /// </summary>
        public Expression Reduce(Expression expression)
        {
            var reduceVisitor = reduceVisitorFactory.Create();
            return reduceVisitor.VisitAndReduce(expression);
        }

        /// <summary>
        /// Dynamic execute for compiled expressions.
        /// </summary>
        /// <param name="info">info, used as key.</param>
        /// <param name="parameters">parameters for func.</param>
        public dynamic Execute(MethodInfo info, params dynamic[] parameters)
        {
            dynamic func = compiledExpressionCache.Get(info);

            if (callDispatchers.ContainsKey(parameters.Length))
            {
                return callDispatchers[parameters.Length](func, parameters);
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
