// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run execution context. Contain information related to tests execution time,
    /// current step, etc.
    /// </summary>
    public class TestRunExecutionContext
    {
        /// <summary>
        /// Current running test.
        /// </summary>
        public TestRun TestRun { get; internal set; }

        /// <summary>
        /// Current step.
        /// </summary>
        public ITestRunStep Step { get; internal set; }

        /// <summary>
        /// When step has started.
        /// </summary>
        public DateTime StartedAt { get; internal set; }

        /// <summary>
        /// Logger.
        /// </summary>
        public ITestRunLogger Logger { get; internal set; }

        /// <summary>
        /// Current step number.
        /// </summary>
        public int StepNumber { get; internal set; }

        /// <summary>
        /// Resolver.
        /// </summary>
        public IServiceProvider ServiceProvider { get; internal set; }

        /// <summary>
        /// First exception occurred during <see cref="TestRun" /> execution.
        /// Usually refers to original error.
        /// </summary>
        public Exception FirstException { get; internal set; }

        /// <summary>
        /// Last exception occurred during <see cref="TestRun" /> execuiton.
        /// </summary>
        public Exception LastException { get; internal set; }

        /// <summary>
        /// Last step execution result.
        /// </summary>
        public object LastResult { get; internal set; }

        /// <summary>
        /// Initialize dependency injection resolver.
        /// </summary>
        /// <param name="serviceProvider">DI resolver.</param>
        public void SetResolver(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Empty execution context.
        /// </summary>
        public static readonly TestRunExecutionContext Empty = new TestRunExecutionContext();
    }
}
