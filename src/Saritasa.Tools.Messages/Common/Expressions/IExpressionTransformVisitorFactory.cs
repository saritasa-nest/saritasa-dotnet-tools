using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public interface IExpressionTransformVisitorFactory
    {
        ExpressionVisitor Create();
    }
}
