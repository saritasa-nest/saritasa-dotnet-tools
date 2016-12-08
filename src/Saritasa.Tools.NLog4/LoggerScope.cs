// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET452
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#else
using System.Threading;
#endif

namespace Saritasa.Tools.NLog
{
    /// <summary>
    /// Scope implementation.
    /// </summary>
    /// <remarks>
    /// Implementation based on https://github.com/aspnet/Logging/blob/dev/src/Microsoft.Extensions.Logging.Console/ConsoleLogScope.cs .
    /// </remarks>
    public class LoggerScope
    {
        readonly object state;

        /// <summary>
        /// Parent scope context or null if current one on top.
        /// </summary>
        public LoggerScope Parent { get; private set; }

#if NET452
        private static readonly string FieldKey = $"{typeof(LoggerScope).FullName}.{AppDomain.CurrentDomain.Id}";

        /// <summary>
        /// Logger scope related to current execution (thread) context.
        /// </summary>
        public static LoggerScope Current
        {
            get
            {
                var handle = CallContext.LogicalGetData(FieldKey) as ObjectHandle;
                return (LoggerScope) handle?.Unwrap();
            }
            set
            {
                CallContext.LogicalSetData(FieldKey, new ObjectHandle(value));
            }
        }
#else
        private static AsyncLocal<LoggerScope> localValue = new AsyncLocal<LoggerScope>();

        /// <summary>
        /// Logger scope related to current execution (thread) context.
        /// </summary>
        public static LoggerScope Current
        {
            set
            {
                localValue.Value = value;
            }

            get
            {
                return localValue.Value;
            }
        }
#endif

        /// <summary>
        /// Set new logger state and make previous one as parent.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns>Disposable scope to unset parent (pop stack).</returns>
        public static IDisposable Push(object state)
        {
            var temp = Current;
            Current = new LoggerScope(state);
            Current.Parent = temp;

            return new DisposableScope();
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="state">State.</param>
        public LoggerScope(object state)
        {
            this.state = state;
        }

        /// <inheritdoc />
        public override string ToString() => state?.ToString();

        /// <summary>
        /// Dummy class to pop scope stack.
        /// </summary>
        private class DisposableScope : IDisposable
        {
            /// <inheritdoc />
            public void Dispose()
            {
                Current = Current.Parent;
            }
        }
    }
}
