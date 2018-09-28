using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public static Task CompletedTask { get; set; }

        static TaskHelpers()
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            CompletedTask = tcs.Task;
        }
    }
}
