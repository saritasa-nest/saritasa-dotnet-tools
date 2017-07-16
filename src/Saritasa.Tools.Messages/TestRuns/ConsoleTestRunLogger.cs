// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Console logger implementation.
    /// </summary>
    public class ConsoleTestRunLogger : ITestRunLogger
    {
        /// <inheritdoc />
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
