using System;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public class ExpressionTransformVisitorFactory : IExpressionTransformFactory
    {
        private ExpressionTransformContext context;

        public ExpressionTransformVisitorFactory(ExpressionTransformContext context)
        {
            this.context = context;
        }

        public ExpressionTransformVisitorFactory() { }

        public ExpressionTransformContext Context => context;

        public ExpressionTransformVisitor Create()
        {
            return new ExpressionTransformVisitor(Context);
        }

        public ExpressionTransformVisitor Create(Action<ExpressionTransformContext> contextConfigure)
        {
            var context = new ExpressionTransformContext();
            contextConfigure(context);

            return new ExpressionTransformVisitor(context);
        }
    }
}
