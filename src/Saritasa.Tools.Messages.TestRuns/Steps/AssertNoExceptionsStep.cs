// Copyright (c) 2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json.Linq;

namespace Saritasa.Tools.Messages.TestRuns.Steps
{
    /// <summary>
    /// Asserts that current context does not have exceptions initialized.
    /// </summary>
    public class AssertNoExceptionsStep : ITestRunStep
    {
        /// <inheritdoc />
        public void Run(TestRunExecutionContext context)
        {
            if (context.FirstException != null || context.LastException != null)
            {
                TestRunAssertException.ThrowWithExecutionContext(
                    "There are exception(-s) from last step(-s) when no expected.", context, this);
            }
        }

        /// <inheritdoc />
        public JObject Save()
        {
            return new JObject();
        }
    }
}
