// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// Scope implementation.
    /// </summary>
    /// <remarks>
    /// Implementation based on https://github.com/aspnet/Logging/blob/master/src/Microsoft.Extensions.Logging.Console/ConsoleLogScope.cs.
    /// </remarks>
    public static class LocalStorage<T> where T : class
    {
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
    }
}
