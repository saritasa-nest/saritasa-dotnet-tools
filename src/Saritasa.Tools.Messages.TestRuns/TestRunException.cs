// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run exception.
    /// </summary>
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class TestRunException : Exception
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public TestRunException()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="message">Exception message.</param>
        public TestRunException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public TestRunException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
