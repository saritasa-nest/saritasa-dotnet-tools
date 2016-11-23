using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Tests.Expressions.Fixtures;
using Xunit;

namespace Saritasa.Tools.Tests.Expressions
{
    public class ExpressionReduceVisitorTests : IClassFixture<ExpressionReduceVisitorFixture>
    {
        private ExpressionReduceVisitorFixture fixture;
        private int testField = 1;

        public ExpressionReduceVisitorTests(ExpressionReduceVisitorFixture fixture)
        {
            this.fixture = fixture;
        }

        private int Sum(int x, int y) => x + y;

        private int Substract(int x, int y) => x - y;

        private ExpressionReduceVisitorTests Chain(int x, int y)
        {
            return this;
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void ExpressionReduceVisitor_visit_and_reduce_should_reduce_add_operation(int value1, int value2, int sum)
        {
            // Arrange
            var visitor = fixture.Create();
            Expression<Func<ExpressionReduceVisitorTests, int>> sumExpression = (v) => v.Sum(value1, value1 + value2);
            Expression<Func<ExpressionReduceVisitorTests, int>> expectedExpression = (v) => v.Sum(value1, 3);

            // Act
            var result = visitor.VisitAndReduce(sumExpression);

            // Assert
            Assert.Equal(expectedExpression.ToString(), result.ToString());
        }

        [Theory]
        [InlineData(6, 1, 1)]
        public void ExpressionReduceVisitor_visit_and_reduce_should_reduce_substract_operation(int value1, int value2, int value3)
        {
            // Arrange
            var visitor = fixture.Create();
            Expression<Func<ExpressionReduceVisitorTests, int>> substract = (v) => v.Substract(value1, value2 - value3);
            Expression<Func<ExpressionReduceVisitorTests, int>> expectedExpression = (v) => v.Substract(value1, 0);

            // Act
            var result = visitor.VisitAndReduce(substract);

            // Assert
            Assert.Equal(expectedExpression.ToString(), result.ToString());
        }

        [Theory]
        [InlineData(6, 6, 2)]
        public void ExpressionReduceVisitor_visit_and_reduce_should_reduce_divide_operation(int value1, int value2, int value3)
        {
            // Arrange
            var visitor = fixture.Create();
            Expression<Func<ExpressionReduceVisitorTests, int>> substract = (v) => v.Substract(value1, value2 / value3);
            Expression<Func<ExpressionReduceVisitorTests, int>> expectedExpression = (v) => v.Substract(value1, 3);

            // Act
            var result = visitor.VisitAndReduce(substract);

            // Assert
            Assert.Equal(expectedExpression.ToString(), result.ToString());
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void ExpressionReduceVisitor_visit_and_reduce_should_not_reduce_member_properties(int value1, int value2, int value3)
        {
            // Arrange
            var visitor = fixture.Create();
            Expression<Func<ExpressionReduceVisitorTests, int>> sumExpression = (v) => v.Substract(v.testField,  value2 * value3);
            Expression<Func<ExpressionReduceVisitorTests, int>> expectedExpression = (v) => v.Substract(v.testField, 6);

            // Act
            var result = visitor.VisitAndReduce(sumExpression);

            // Assert
            Assert.Equal(expectedExpression.ToString(), result.ToString());
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void ExpressionReduceVisitor_visit_and_reduce_should_reduce_arguments_int_chained_calls(int value1, int value2, int value3)
        {
            // Arrange
            var visitor = fixture.Create();
            Expression<Func<ExpressionReduceVisitorTests, ExpressionReduceVisitorTests>> sumExpression = (v) => v.Chain(v.testField, value2 * value3)
            .Chain(v.testField, value2 * value3)
            .Chain(v.testField, value2 * value3);
            Expression<Func<ExpressionReduceVisitorTests, ExpressionReduceVisitorTests>> expectedExpression = (v) => v.Chain(v.testField, 6)
            .Chain(v.testField, 6)
            .Chain(v.testField, 6);

            // Act
            var result = visitor.VisitAndReduce(sumExpression);

            Assert.Equal(expectedExpression.ToString(), result.ToString());
        }
    }
}
