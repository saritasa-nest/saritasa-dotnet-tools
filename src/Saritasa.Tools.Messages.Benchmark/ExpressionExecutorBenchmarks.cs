using BenchmarkDotNet.Attributes;
using Saritasa.Tools.Messages.Common.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Saritasa.Tools.Messages.Benchmark
{
    public class ExpressionExecutorBenchmarks
    {
        private static Expression<Func<ExpressionExecutorBenchmarks, int, int, int>> sumExpression = (v, p0, p1) => v.Sum(p0, p1);
        private ExpressionExecutorServices serviceProvider = new ExpressionExecutorServices();
        private MethodInfo methodInfo = typeof(ExpressionExecutorBenchmarks).GetMethod(nameof(ExpressionExecutorBenchmarks.Sum), BindingFlags.NonPublic | BindingFlags.Instance);
        private ExpressionExecutor executor;

        private int Sum(int v0, int v1) => v0 + v1;

        public ExpressionExecutorBenchmarks()
        {
            var factory = new ExpressionExecutorFactory(serviceProvider);
            executor = factory.Create();
            executor.PreCompile(sumExpression);
        }

        [Benchmark]
        public void RunCompiledExpressionGenericSum()
        {
            Enumerable.Range(1, 10000)
                .Aggregate((cur, next) =>
                {
                    var result = executor.ExecuteTyped<ExpressionExecutorBenchmarks, int, int, int>(methodInfo, this, cur, next);

                    return result;
                });
        }

        [Benchmark]
        public void RunCompiledExpressionNonGenericSum()
        {
            Enumerable.Range(1, 10000)
                .Aggregate((cur, next) =>
                {
                    var result = executor.Execute(methodInfo, this, cur, next);

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
