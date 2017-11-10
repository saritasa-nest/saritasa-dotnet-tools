// Copyright (c) 2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run loader.
    /// </summary>
    public interface ITestRunLoader
    {
        /// <summary>
        /// Get test runs.
        /// </summary>
        /// <returns>Enumeration of test runs.</returns>
        IEnumerable<TestRun> Get();
    }
}
