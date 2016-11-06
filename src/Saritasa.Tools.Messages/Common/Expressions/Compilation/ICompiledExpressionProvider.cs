using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    public interface ICompiledExpressionProvider
    {
        IReadOnlyDictionary<MethodInfo, dynamic> KeyValues { get; }

        Func<TInput, TResult> GetOrAdd<TInput, TResult>(MethodInfo methodInfo, Func<dynamic> factory);
    }
}
