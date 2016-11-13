using BenchmarkDotNet.Attributes;
using Saritasa.Tools.Messages.Common.Expressions;
using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using Saritasa.Tools.Messages.Common.Expressions.Transformers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Saritasa.Tools.Messages.Benchmark
{
    public class ExpressionExecutorBenchmarks
    {
        private Expression<Func<ExpressionExecutorBenchmarks, int, int, int>> sumExpression = (v, p0, p1) => v.Sum(p0, p1);
        private ServiceProvider serviceProvider = new ServiceProvider();
        private MethodInfo methodInfo = typeof(ExpressionExecutorBenchmarks).GetMethod(nameof(ExpressionExecutorBenchmarks.Sum), BindingFlags.NonPublic | BindingFlags.Instance);

        private int Sum(int v0, int v1) => v0 + v1;

        private class ServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ICompiledExpressionCache))
                {
                    return new CompiledExpressionCache();
                }

                if (serviceType == typeof(IExpressionCompilator))
                {
                    return new ExpressionCompilator();
                }

                if (serviceType == typeof(IExpressionTransformVisitorFactory))
                {
                    return new ExpressionTransformVisitorFactory(new List<IExpressionTransformer>() {
                        new LambdaExpressionTransformer(),
                        new MethodCallExpressionTransformer()
                    });
                }

                return null;
            }
        }

        [Benchmark]
        public void RunCompiledExpressionSum()
        {
            var factory = new ExpressionExecutorFactory(serviceProvider);
            var expressionExecutor = factory.Create();
            expressionExecutor.PreCompile(sumExpression);


            Enumerable.Range(1, 10000)
                .Aggregate((cur, next) =>
                {
                    var result = expressionExecutor.Execute<ExpressionExecutorBenchmarks, int, int, int>(methodInfo, this, cur, next);

                    return result;
                });
        }

        [Benchmark]
        public void RunCompiledExpressionNonGenericSum()
        {
            var factory = new ExpressionExecutorFactory(serviceProvider);
            var expressionExecutor = factory.Create();
            expressionExecutor.PreCompile(sumExpression);


            Enumerable.Range(1, 10000)
                .Aggregate((cur, next) =>
                {
                    var result = expressionExecutor.Execute(methodInfo, this, cur, next);

                    return (int)result;
                });
        }

        [Benchmark]
        public void RunReflectionSum()
        {
            Enumerable.Range(1, 10000)
                .Aggregate((cur, next) =>
                {

                    var result = methodInfo.Invoke(this, new object[] { cur, next });

                    return (int)result;
                });
        }


    }
}
