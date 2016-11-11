using Saritasa.Tools.Messages.Common.Expressions;
using Saritasa.Tools.Messages.Common.Expressions.Transformers;
using System.Collections.Generic;

namespace Saritasa.Tools.Tests.Expressions.Fixtures
{
    public class ExpressionTransformVisitorFixture
    {
        public ExpressionTransformVisitor CreateWithAllTransformers()
        {
            return new ExpressionTransformVisitor(new List<IExpressionTransformer>()
                {
                    new MethodCallExpressionTransformer(),
                    new LambdaExpressionTransformer()
                });
        }
    }
}
