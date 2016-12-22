using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Common.Expressions;
using Saritasa.Tools.Messages.Common.Expressions.Transformers;

namespace Saritasa.Tools.Messages.Tests.Expression.Fixtures
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
