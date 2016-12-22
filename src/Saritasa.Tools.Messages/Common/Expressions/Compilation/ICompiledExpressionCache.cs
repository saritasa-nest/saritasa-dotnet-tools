using System;
using System.Reflection;

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
        void Add(MethodInfo methodInfo, Func<Delegate> factory);

        /// <summary>
        /// Getting already containing delegate.
        /// </summary>
        Delegate Get(MethodInfo methodInfo);

        /// <summary>
        /// Checking up existence of compiled expression.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        bool HasKey(MethodInfo methodInfo);

        /// <summary>
        /// Clearing cache.
        /// </summary>
        void Clear();
    }
}
