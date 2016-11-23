using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Common.Expressions.Reduce;

namespace Saritasa.Tools.Tests.Expressions.Fixtures
{
    public class ExpressionReduceVisitorFixture
    {
        public ExpressionReduceVisitor Create()
        {
            return new ExpressionReduceVisitor();
        }
    }
}
