using Saritasa.Tools.Tests.Expressions.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public int CompiledMethod(int first)
        {
            return first + 1;
        }

        [Fact]
        public void Expression_executor_should_correct_compile_and_execute_method()
        {
            // Arrange
            var executor = fixture.Create();

            var compiled = executor.Compile<int, int>((first) => CompiledMethod(first));

            // Act
            var executed = executor.Execute((first) => CompiledMethod(first), 1);

            // Assert
            Assert.Equal(1, executor.Context.CompiledExpressionProvider.KeyValues.Count);
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(2, 3)]
        [InlineData(3, 4)]
        public void Expression_executor_with_one_method_should_compile_once_and_execute_twice(int firstValue, int result)
        {
            // Arrange
            var executor = fixture.Create();

            var compiled = executor.Compile<int, int>((first) => CompiledMethod(first));

            // Act
            var executed = executor.Execute((first) => CompiledMethod(first), firstValue);

            // Assert
            Assert.Equal(result, executed);
            Assert.Equal(1, executor.Context.CompiledExpressionProvider.KeyValues.Count);
        }
    }
}
