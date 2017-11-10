// Copyright (c) 2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Extensions for <see cref="ITestRunLogger" />.
    /// </summary>
    public static class TestRunLoggerExtensions
    {
        /// <summary>
        /// Log text with current date time prepend.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="log">Log text.</param>
        public static void LogWithTime(this ITestRunLogger logger, string log)
        {
            logger.Log($"[{DateTime.Now:yy-MM-dd hh:mm:ss}] {log}");
        }
    }
}
