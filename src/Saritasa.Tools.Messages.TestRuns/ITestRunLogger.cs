// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run logger.
    /// </summary>
    public interface ITestRunLogger
    {
        /// <summary>
        /// Log message.
        /// </summary>
        /// <param name="message">Message.</param>
        void Log(string message);
    }
}
