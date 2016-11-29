using System;
using System.Linq.Expressions;
using Saritasa.Tools.Messages.Tests.Expression.Fixtures;
using Xunit;

namespace Saritasa.Tools.Messages.Tests.Expression
{
    public class ExpressionTransformVisitorTests : IClassFixture<ExpressionTransformVisitorFixture>
    {
        private ExpressionTransformVisitorFixture fixture;

        public ExpressionTransformVisitorTests(ExpressionTransformVisitorFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void ExpressionTransformVisitor_with_zero_arguments_on_method_should_not_transform_anything()
        {
            // Arrange
            var visitor = fixture.CreateWithAllTransformers();
            Expression<Action<ExpressionTransformVisitorTests>> expectedAction = (v) =>
                v.ExpressionTransformVisitor_with_zero_arguments_on_method_should_not_transform_anything();

            // Act
            var resultAction = visitor.Visit(expectedAction);

            // Assert
            Assert.Equal(expectedAction.ToString(), resultAction.ToString()); // should be same.
        }

        [Theory]
        [InlineData(1, 2, 3, 4)]
        [InlineData(4, 3, 2, 1)]
        public void ExpressionTransformVisitor_with_constants_should_replace_constats_to_parameters(int param1, int param2, int param3, int param4)
        {
            // Arrange
            var visitor = fixture.CreateWithAllTransformers();
            Expression<Action<ExpressionTransformVisitorTests, int, int, int, int>> expectedAction = (v, p0, p1, p2, p3) =>
                v.ExpressionTransformVisitor_with_constants_should_replace_constats_to_parameters(p0, p1, p2, p3);

            Expression<Action<ExpressionTransformVisitorTests>> transformingAction = (v) =>
                v.ExpressionTransformVisitor_with_constants_should_replace_constats_to_parameters(param1, param2, param3, param4);

            // Act
            var resultAction = visitor.Visit(transformingAction);

            // Assert
            Assert.Equal(expectedAction.ToString(), resultAction.ToString()); // should be same.
        }

        [Theory]
        [InlineData(1, 2, 3, null)]
        [InlineData(3, 2, 1, null)]
        public void ExpressionTransformVisitor_with_constants_and_one_null_should_replace_to_typed_parameters(int param1, int param2, int param3, object param4)
        {
            // Arrange
            var visitor = fixture.CreateWithAllTransformers();
            Expression<Action<ExpressionTransformVisitorTests, int, int, int, object>> expectedAction = (v, p0, p1, p2, p3) =>
                v.ExpressionTransformVisitor_with_constants_and_one_null_should_replace_to_typed_parameters(p0, p1, p2, p3);

            Expression<Action<ExpressionTransformVisitorTests>> transformingAction = (v) =>
                v.ExpressionTransformVisitor_with_constants_and_one_null_should_replace_to_typed_parameters(param1, param2, param3, param4);

            // Act
            var resultAction = visitor.Visit(transformingAction);

            // Assert
            Assert.Equal(expectedAction.ToString(), resultAction.ToString()); // should be same.
        }

        [Theory]
        [InlineData(1, 2, 3, null)]
        [InlineData(3, 2, 1, null)]
        public void ExpressionTransformVisitor_with_constants_and_one_default_should_replace_to_typed_parameters(int param1, int param2, int param3, DateTime param4)
        {
            // Arrange
            var visitor = fixture.CreateWithAllTransformers();
            Expression<Action<ExpressionTransformVisitorTests, int, int, int, DateTime>> expectedAction = (v, p0, p1, p2, p3) =>
                v.ExpressionTransformVisitor_with_constants_and_one_default_should_replace_to_typed_parameters(p0, p1, p2, p3);

            Expression<Action<ExpressionTransformVisitorTests>> transformingAction = (v) =>
                v.ExpressionTransformVisitor_with_constants_and_one_default_should_replace_to_typed_parameters(param1, param2, param3, default(DateTime));

            // Act
            var resultAction = visitor.Visit(transformingAction);

            // Assert
            Assert.Equal(expectedAction.ToString(), resultAction.ToString()); // should be same.
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(3, 2, 1)]
        public void ExpressionTransformVisitor_with_constants_should_replace_to_typed_parameters(int param1, int param2, int param3)
        {
            // Arrange
            var visitor = fixture.CreateWithAllTransformers();
            Expression<Action<ExpressionTransformVisitorTests, int, int, int>> expectedAction = (v, p0, p1, p2) =>
                v.ExpressionTransformVisitor_with_constants_should_replace_to_typed_parameters(p0, p1, p2);

            Expression<Action<ExpressionTransformVisitorTests>> transformingAction = (v) =>
                v.ExpressionTransformVisitor_with_constants_should_replace_to_typed_parameters(1, 2, 3);

            // Act
            var resultAction = visitor.Visit(transformingAction);

            // Assert
            Assert.Equal(expectedAction.ToString(), resultAction.ToString());
        }
    }
}
