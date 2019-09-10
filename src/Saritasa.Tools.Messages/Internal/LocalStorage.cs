// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET452
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#else
using System.Threading;
#endif

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// Scope implementation.
    /// </summary>
    /// <remarks>
    /// Implementation based on https://github.com/aspnet/Logging/blob/dev/src/Microsoft.Extensions.Logging.Console/ConsoleLogScope.cs .
    /// </remarks>
    public static class LocalStorage<T> where T : class
    {
#if NET452
        private static readonly string FieldKey = $"{typeof(T).FullName}.{AppDomain.CurrentDomain.Id}";

        /// <summary>
        /// State related to current execution (thread) context.
        /// </summary>
        public static T Current
        {
            get
            {
                var handle = CallContext.LogicalGetData(FieldKey) as ObjectHandle;
                return (T)handle?.Unwrap();
            }

            set
            {
                CallContext.LogicalSetData(FieldKey, new ObjectHandle(value));
            }
        }
#else
        private static readonly AsyncLocal<T> localValue = new AsyncLocal<T>();

        /// <summary>
        /// State related to current execution (thread) context.
        /// </summary>
        public static T Current
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
    }
}
