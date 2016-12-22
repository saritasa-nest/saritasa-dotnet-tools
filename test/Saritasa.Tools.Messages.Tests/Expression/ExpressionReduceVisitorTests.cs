using System;
using System.Linq.Expressions;
using Saritasa.Tools.Messages.Tests.Expression.Fixtures;
using Xunit;

namespace Saritasa.Tools.Messages.Tests.Expression
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
            Expression<Func<ExpressionReduceVisitorTests, int>> expression = (v) => v.Substract(v.testField, value2 * value3);
            Expression<Func<ExpressionReduceVisitorTests, int>> expectedExpression = (v) => v.Substract(v.testField, 6);

            // Act
            var result = visitor.VisitAndReduce(expression);

            // Assert
            Assert.Equal(expectedExpression.ToString(), result.ToString());
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void ExpressionReduceVisitor_visit_and_reduce_should_reduce_arguments_int_chained_calls(int value1, int value2, int value3)
        {
            // Arrange
            var visitor = fixture.Create();
            Expression<Func<ExpressionReduceVisitorTests, ExpressionReduceVisitorTests>> multiplyExpression = (v) => v.Chain(v.testField, value2 * value3)
            .Chain(v.testField, value2 * value3)
            .Chain(v.testField, value2 * value3);
            Expression<Func<ExpressionReduceVisitorTests, ExpressionReduceVisitorTests>> expectedExpression = (v) => v.Chain(v.testField, 6)
            .Chain(v.testField, 6)
            .Chain(v.testField, 6);

            // Act
            var result = visitor.VisitAndReduce(multiplyExpression);

            // Assert
            Assert.Equal(expectedExpression.ToString(), result.ToString());
        }

        [Theory]
        [InlineData(10, 20, 60)]
        public void ExpressionReduceVisitor_visit_and_reduce_different_expressions_should_execute_correct(int value1, int value2, int value3)
        {
            // Arrange
            var visitor = fixture.Create();
            Expression<Func<ExpressionReduceVisitorTests, ExpressionReduceVisitorTests>> multiplyExpression = (v) => v.Chain(v.testField, value2 * value3)
            .Chain(v.testField, value2 * value3)
            .Chain(v.testField, value2 * value3);
            Expression<Func<ExpressionReduceVisitorTests, int>> multiplyExpression2 = (v) => v.Substract(v.testField, value2 * value3);
            Expression<Func<ExpressionReduceVisitorTests, int>> divideExpression = (v) => v.Substract(value1, value3 / value2);
            
            Expression<Func<ExpressionReduceVisitorTests, ExpressionReduceVisitorTests>> expectedMultiplyExpression = (v) => v.Chain(v.testField, 1200)
            .Chain(v.testField, 1200)
            .Chain(v.testField, 1200);
            Expression<Func<ExpressionReduceVisitorTests, int>> expectedMultiplyExpression2 = (v) => v.Substract(v.testField, 1200);
            Expression<Func<ExpressionReduceVisitorTests, int>> expectedDivideExpression = (v) => v.Substract(value1, 3);


            // Act
            var multiplyExpressionResult = visitor.VisitAndReduce(multiplyExpression);
            var multiplyExpression2Result = visitor.VisitAndReduce(multiplyExpression2);
            var divideExpressionResult = visitor.VisitAndReduce(divideExpression);

            multiplyExpressionResult = visitor.VisitAndReduce(multiplyExpression);
            multiplyExpression2Result = visitor.VisitAndReduce(multiplyExpression2);
            divideExpressionResult = visitor.VisitAndReduce(divideExpression);

            // Assert
            Assert.Equal(expectedMultiplyExpression.ToString(), multiplyExpressionResult.ToString());
            Assert.Equal(expectedMultiplyExpression2.ToString(), multiplyExpression2Result.ToString());
            Assert.Equal(expectedDivideExpression.ToString(), divideExpressionResult.ToString());
        }
    }
}