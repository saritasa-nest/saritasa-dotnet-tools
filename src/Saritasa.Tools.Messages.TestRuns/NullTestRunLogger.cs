// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Null logger.
    /// </summary>
    public class NullTestRunLogger : ITestRunLogger
    {
        /// <inheritdoc />
        public void Log(string message)
        {
        }
    }
}
