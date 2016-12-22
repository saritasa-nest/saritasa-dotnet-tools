using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using Saritasa.Tools.Messages.Common.Expressions.Reduce;
using Saritasa.Tools.Messages.Common.Expressions.Transformers;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    /// <summary>
    /// Services related to expression executor.
    /// </summary>
    public class ExpressionExecutorServices
    {
        private static ExpressionExecutorServices instance;
        private static object @lock = new object();

        /// <summary>
        /// Ctor
        /// </summary>
        private ExpressionExecutorServices()
        {
            CompiledCache = new CompiledExpressionCache();
            ExpressionCompilator = new ExpressionCompilator();
            TransformVisitorFactory = new ExpressionTransformVisitorFactory(new List<IExpressionTransformer>(2)
            {
                new LambdaExpressionTransformer(),
                new MethodCallExpressionTransformer()
            });
            ReduceVisitor = new ExpressionReduceVisitor();
        }

        public ICompiledExpressionCache CompiledCache { get; set; }

        public IExpressionCompilator ExpressionCompilator { get; set; }

        public IExpressionTransformVisitorFactory TransformVisitorFactory { get; set; }

        public IExpressionReduceVisitor ReduceVisitor { get; set; }

        public static ExpressionExecutorServices Instance
        {
            get
            {
                lock (@lock)
                {
                    return instance ?? (instance = new ExpressionExecutorServices());
                }
            }
        }
    }
}
