using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using Saritasa.Tools.Messages.Common.Expressions.Transformers;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public class ExpressionExecutorServices
    {
        private static ExpressionExecutorServices instance;
        private static object @lock = new object();

        public ExpressionExecutorServices()
        {
            CompiledCache = new CompiledExpressionCache();
            ExpressionCompilator = new ExpressionCompilator();
            TransformVisitorFactory = new ExpressionTransformVisitorFactory(new List<IExpressionTransformer>()
            {
                new LambdaExpressionTransformer(),
                new MethodCallExpressionTransformer()
            });
            ReduceVisitorFactory = new ExpressionReduceVisitorFactory();
        }

        public ICompiledExpressionCache CompiledCache { get; set; }

        public IExpressionCompilator ExpressionCompilator { get; set; }

        public IExpressionTransformVisitorFactory TransformVisitorFactory { get; set; }

        public IExpressionReduceVisitorFactory ReduceVisitorFactory { get; set; }

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
