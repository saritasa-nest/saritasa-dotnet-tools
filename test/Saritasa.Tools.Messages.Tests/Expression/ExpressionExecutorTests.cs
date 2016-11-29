using System;
using System.Linq.Expressions;
using System.Reflection;
using Saritasa.Tools.Messages.Common.Expressions;
using Saritasa.Tools.Messages.Tests.Expression.Fixtures;
using Xunit;

namespace Saritasa.Tools.Messages.Tests.Expression
{
    public class ExpressionExecutorTests : IClassFixture<ExpressionExecutorFixture>
    {
        private ExpressionExecutorFixture fixture;
        private ExpressionExecutor executor;

        public ExpressionExecutorTests(ExpressionExecutorFixture fixture)
        {
            this.fixture = fixture;
            this.executor = fixture.Create();
        }

        public int CompiledMethod(int first, int second)
        {
            return first + second;
        }

        public ExpressionExecutorTests SimpleReturn(int value, int value2)
        {
            return this;
        }

        [Fact]
        public void Expression_executor_with_one_method_should_compile_and_reduce()
        {
            // Arrange
            Expression<Func<ExpressionExecutorTests, int>> expression = (v) => CompiledMethod(10 + 1, 1 + 3);

            // Act
            executor.CompiledCache.Clear();
            executor.PreCompile(expression);
            executor.PreCompile(expression);
            executor.PreCompile(expression);

            // Assert
            Assert.Equal(1, executor.CompiledCache.Count);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void Expression_executor_with_one_method_and_outer_parameters_should_compile(int value0, int value1, int result)
        {
            // Arrange
            Expression<Func<ExpressionExecutorTests, int>> expression = (v) => CompiledMethod(value0, value1);

            // Act
            executor.CompiledCache.Clear();
            executor.PreCompile(expression);
            executor.PreCompile(expression);
            executor.PreCompile(expression);

            // Assert
            Assert.Equal(1, executor.CompiledCache.Count);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void Expression_executor_with_one_method_and_outer_parameters_should_compile_and_execute_correctly(int value0, int value1, int result)
        {
            // Arrange
            Expression<Func<ExpressionExecutorTests, int>> expression = (v) => CompiledMethod(value0, value1);
            var methodInfo = typeof(ExpressionExecutorTests).GetMethod("CompiledMethod");

            // Act
            executor.CompiledCache.Clear();
            executor.PreCompile(expression);
            executor.PreCompile(expression);
            executor.PreCompile(expression);

            var executed = executor.ExecuteTyped<ExpressionExecutorTests, int, int, int>(methodInfo, this, value0, value1);

            // Assert
            Assert.Equal(result, executed);
            Assert.Equal(1, executor.CompiledCache.Count);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void Expression_execute_non_typed_with_one_method_and_outer_parameters_should_compile_and_execute_correctly(int value0, int value1, int result)
        {
            // Arrange
            Expression<Func<ExpressionExecutorTests, int>> expression = (v) => CompiledMethod(value0, value1);
            var methodInfo = typeof(ExpressionExecutorTests).GetMethod("CompiledMethod");

            // Act
            executor.CompiledCache.Clear();
            executor.PreCompile(expression);
            executor.PreCompile(expression);
            executor.PreCompile(expression);

            var executed = executor.Execute(methodInfo, this, value0, value1);

            // Assert
            Assert.Equal(result, executed);
            Assert.Equal(1, executor.CompiledCache.Count);
        }

        [Theory]
        [InlineData(1, 2, 6)]
        public void Expression_reduce_and_execute_with_one_method_and_outer_parameters_should_compile_and_execute_correctly(int value0, int value1, int result)
        {
            // Arrange
            Expression<Func<ExpressionExecutorTests, int>> expression = (v) => CompiledMethod(value0 + 1, value1 + 2);
            var methodInfo = typeof(ExpressionExecutorTests).GetMethod("CompiledMethod");

            // Act
            executor.CompiledCache.Clear();
            var reduceResult = executor.Reduce(expression);
            expression = reduceResult;
            executor.PreCompile(expression);
            executor.PreCompile(expression);
            executor.PreCompile(expression);

            var executed = executor.Execute(methodInfo, this, value0 + 1, value1 + 2);
            // Assert
            Assert.Equal(result, executed);
            Assert.Equal(1, executor.CompiledCache.Count);
        }

        [Theory]
        [InlineData(1, 2, 4)]
        public void Expression_reduce_and_execute_with_one_method_and_outer_parameters_which_one_is_binary_should_compile_and_execute_correctly(int value0, int value1, int result)
        {
            // Arrange
            Expression<Func<ExpressionExecutorTests, int>> expression = (v) => CompiledMethod(value0 + 1, value1);
            var methodInfo = typeof(ExpressionExecutorTests).GetMethod("CompiledMethod");

            // Act
            executor.CompiledCache.Clear();
            expression = executor.Reduce(expression);
            executor.PreCompile(expression);
            executor.PreCompile(expression);
            executor.PreCompile(expression);

            var executed = executor.Execute(methodInfo, this, 2, value1);

            // Assert
            Assert.Equal(result, executed);
            Assert.Equal(1, executor.CompiledCache.Count);
        }

        [Fact]
        public void Expression_reduce_and_execute_with_complex_outer_parameter_should_reduce_correctly()
        {
            // Arrange
            var testClass = fixture.CreateTestObject();
            Expression<Func<ExpressionExecutorTests, int>> expression = (v) => CompiledMethod(testClass.Value1 + 1, testClass.Value2);
            Expression<Func<ExpressionExecutorTests, int>> expression2 = (v) => CompiledMethod(testClass.Value1 + 1, testClass.Value2 + 1);
            Expression<Func<ExpressionExecutorTests, int>> expression3 = (v) => CompiledMethod(testClass.Value1, testClass.Value2 + 1);
            var methodInfo = typeof(ExpressionExecutorTests).GetMethod("CompiledMethod");

            // Act
            executor.CompiledCache.Clear();
            expression = executor.Reduce(expression);
            expression2 = executor.Reduce(expression2);
            expression3 = executor.Reduce(expression3);
            executor.PreCompile(expression);
            executor.PreCompile(expression2);
            executor.PreCompile(expression3);

            var executed = executor.Execute(methodInfo, this, 2, 2);
            var executed2 = executor.Execute(methodInfo, this, 2, 3);
            var executed3 = executor.Execute(methodInfo, this, 1, 3);

            // Assert
            Assert.Equal(4, executed);
            Assert.Equal(5, executed2);
            Assert.Equal(4, executed3);
        }

        [Fact]
        public void Expression_reduce_and_execute_with_complex_outer_parameter_and_chain_method_should_reduce_and_execute_correctly()
        {
            // Arrange
            var testClass = fixture.CreateTestObject();
            Expression<Func<ExpressionExecutorTests, ExpressionExecutorTests>> expression = (v) => SimpleReturn(testClass.Value1 + 1, testClass.Value2)
            .SimpleReturn(testClass.Value1, testClass.Value2)
            .SimpleReturn(testClass.Value2 + 1, testClass.Value2 + 1);
            Expression<Func<ExpressionExecutorTests, ExpressionExecutorTests>> expectedExpression = (v) => SimpleReturn(2, testClass.Value2)
            .SimpleReturn(testClass.Value1, testClass.Value2)
            .SimpleReturn(3, 3);

            // Act
            executor.CompiledCache.Clear();
            expression = executor.Reduce(expression);

            // Assert
            Assert.Equal(expectedExpression.ToString(), expression.ToString());
        }
    }
}