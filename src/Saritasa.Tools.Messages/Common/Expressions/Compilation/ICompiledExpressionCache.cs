using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    /// <summary>
    /// Cache of delegates.
    /// </summary>
    public interface ICompiledExpressionCache
    {
        /// <summary>
        /// Count of compiled delegates.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Getting of adding delegate.
        /// </summary>
        Delegate GetOrAdd(MethodInfo methodInfo, Func<Delegate> factory);

        /// <summary>
        /// Getting already containing delegate.
        /// </summary>
        Delegate Get(MethodInfo methodInfo);

        void Clear();
    }
}
