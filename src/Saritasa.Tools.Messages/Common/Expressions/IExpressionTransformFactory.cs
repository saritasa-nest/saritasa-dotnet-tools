using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public interface IExpressionTransformFactory
    {
        ExpressionTransformVisitor Create();
    }
}
