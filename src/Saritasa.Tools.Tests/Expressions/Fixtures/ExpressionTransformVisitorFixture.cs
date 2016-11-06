using Saritasa.Tools.Messages.Common.Expressions;
using Saritasa.Tools.Messages.Common.Expressions.Transformers;

namespace Saritasa.Tools.Tests.Expressions.Fixtures
{
    public class ExpressionTransformVisitorFixture
    {
        public ExpressionTransformVisitor CreateWithAllTransformers()
        {
            var factory = new ExpressionTransformVisitorFactory();
            return factory.Create(cfg =>
            {
                cfg.UseTransfomer<MethodCallExpressionTransformer>();
                cfg.UseTransfomer<LambdaExpressionTransformer>();
            });
        }
    }
}
