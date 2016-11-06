using Saritasa.Tools.Messages.Common.Expressions;
using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using Saritasa.Tools.Messages.Common.Expressions.Transformers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Tests.Expressions.Fixtures
{
    public class ExpressionExecutorFixture
    {
        public ExpressionExecutor Create()
        {
            var factory = new ExpressionExecutorFactory();
            return factory.Create(cf =>
            {
                cf.UseCompiledCache<CompiledExpressionProvider>();

                cf.ConfigureTransformation(etc =>
                {
                    etc.UseTransfomer<MethodCallExpressionTransformer>();
                    etc.UseTransfomer<LambdaExpressionTransformer>();
                });
            });
        }
    }
}
