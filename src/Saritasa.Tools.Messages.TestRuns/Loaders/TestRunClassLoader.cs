// Copyright (c) 2017-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Messages.TestRuns.Loaders
{
    /// <summary>
    /// The loader allows to retrieve already preloaded instances of <see cref="TestRun" />.
    /// </summary>
    public class TestRunClassLoader : ITestRunLoader
    {
        private readonly TestRun[] testRuns;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="testRuns">Array of preloaded testruns.</param>
        public TestRunClassLoader(params TestRun[] testRuns)
        {
            if (testRuns == null)
            {
                throw new ArgumentNullException(nameof(testRuns));
            }
            this.testRuns = testRuns;
        }

        /// <inheritdoc />
        public IEnumerable<TestRun> Get()
        {
            return testRuns;
        }
    }
}
