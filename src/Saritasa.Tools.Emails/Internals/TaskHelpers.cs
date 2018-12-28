// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace Saritasa.Tools.Emails.Internals
{
    /// <summary>
    /// Internal <see cref="Task" /> helpers.
    /// </summary>
    internal static class TaskHelpers
    {
        /// <summary>
        /// Instance of completed <see cref="Task" />.
        /// </summary>
#if NET452
        public static Task CompletedTask { get; }
#else
        public static Task CompletedTask
        {
            get { return Task.CompletedTask; }
        }
#endif

#if NET452
        static TaskHelpers()
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            CompletedTask = tcs.Task;
        }
#endif
    }
}
