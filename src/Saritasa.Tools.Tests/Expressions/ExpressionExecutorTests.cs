using Saritasa.Tools.Tests.Expressions.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Saritasa.Tools.Tests.Expressions
{
    public class ExpressionExecutorTests : IClassFixture<ExpressionExecutorFixture>
    {
        private ExpressionExecutorFixture fixture;

        public ExpressionExecutorTests(ExpressionExecutorFixture fixture)
        {
            this.fixture = fixture;
        }

        public int CompiledMethod(int first, int second)
        {
            return first + second;
        }

        public int SimpleReturn(int value, int value2)
        {
            return value + value2;
        }

        [Fact]
        public void Expression_executor_with_one_method_should_compile_and_reduce()
        {
            // Arrange
            var executor = fixture.Create();
            Expression<Func<ExpressionExecutorTests, int>> expression = (v) => SimpleReturn(10 + 1, 1 + 3);

            // Act
            executor.ClearCache();
            executor.PreCompile(expression);
            executor.PreCompile(expression);
            executor.PreCompile(expression);

            // Assert
            Assert.Equal(1, executor.CacheCount);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void Expression_executor_with_one_method_and_outer_parameters_should_compile(int value0, int value1, int result)
        {
            // Arrange
            var executor = fixture.Create();
            Expression<Func<ExpressionExecutorTests, int>> expression = (v) => CompiledMethod(value0, value1);

            // Act
            executor.ClearCache();
            executor.PreCompile(expression);
            executor.PreCompile(expression);
            executor.PreCompile(expression);

            // Assert
            Assert.Equal(1, executor.CacheCount);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void Expression_executor_with_one_method_and_outher_parameters_should_compile_and_execute_correctly(int value0, int value1, int result)
        {
            // Arrange
            var executor = fixture.Create();
            Expression<Func<ExpressionExecutorTests, int>> expression = (v) => CompiledMethod(value0, value1);
            var methodInfo = typeof(ExpressionExecutorTests).GetMethod("CompiledMethod");

            // Act
            executor.ClearCache();
            executor.PreCompile(expression);
            executor.PreCompile(expression);
            executor.PreCompile(expression);

            var executed = executor.Execute<ExpressionExecutorTests, int, int, int>(methodInfo, this, value0, value1);

            // Assert
            Assert.Equal(result, executed);
            Assert.Equal(1, executor.CacheCount);
        }
    }
}
