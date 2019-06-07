// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Exception occurs when assert fails during test run.
    /// </summary>
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class TestRunAssertException : TestRunException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TestRunAssertException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public TestRunAssertException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public TestRunAssertException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Throw exception and format it with current execution context.
        /// </summary>
        /// <param name="message">Message to display to user.</param>
        /// <param name="context">Current test execution context.</param>
        /// <param name="step">Test run step instance.</param>
        public static void ThrowWithExecutionContext(string message, TestRunExecutionContext context, ITestRunStep step)
        {
            throw new TestRunAssertException(message);
        }
    }
}
