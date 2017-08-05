// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json.Linq;

namespace Saritasa.Tools.Messages.TestRuns.Steps
{
    /// <summary>
    /// Assert that last step has result.
    /// </summary>
    public class AssertHasResultStep : ITestRunStep
    {
        /// <inheritdoc />
        public void Run(TestRunExecutionContext context)
        {
            if (context.LastResult == null)
            {
                TestRunAssertException.ThrowWithExecutionContext("Expected to have result.", context, this);
            }
        }

        /// <inheritdoc />
        public JObject Save()
        {
            return new JObject();
        }
    }
}
