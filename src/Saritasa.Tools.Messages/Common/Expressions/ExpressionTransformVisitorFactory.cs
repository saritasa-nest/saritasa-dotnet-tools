using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public class ExpressionTransformVisitorFactory : IExpressionTransformVisitorFactory
    {
        private IReadOnlyList<IExpressionTransformer> expressionTransformers;

        public ExpressionTransformVisitorFactory(IReadOnlyList<IExpressionTransformer> transformers)
        {
            this.expressionTransformers = expressionTransformers;
        }

        public ExpressionVisitor Create()
        {
            return new ExpressionTransformVisitor(expressionTransformers);
        }
    }
}
