// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run results.
    /// </summary>
    public class TestRunResult
    {
        /// <summary>
        /// Was test run succeed.
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Exception if any occured.
        /// </summary>
        public Exception FailException { get; set; }
    }
}
