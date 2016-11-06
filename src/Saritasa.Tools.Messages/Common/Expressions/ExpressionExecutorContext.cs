using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public class ExpressionExecutorContext
    {
        public ExpressionTransformContext TransformContext { get; internal set; }

        public IExpressionTransformFactory TransformerFactory { get; internal set; }

        public ICompiledExpressionProvider CompiledExpressionProvider { get; internal set; }

        public ExpressionCompilator Compilator { get; internal set; }
    }
}
