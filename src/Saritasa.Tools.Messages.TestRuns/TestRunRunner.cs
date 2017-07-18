// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run runner. User should implement this class to perform necessary
    /// operations for every test run.
    /// </summary>
    public abstract class TestRunRunner
    {
        /// <summary>
        /// Setup and initialize tests environment. Setup context resolver if needed.
        /// </summary>
        /// <param name="context">Test run execution context.</param>
        public virtual void OnInitialize(TestRunExecutionContext context)
        {
        }

        /// <summary>
        /// The method is called before every test run. Here you can implement all
        /// preparation steps for test, for example init test data.
        /// </summary>
        /// <param name="context">Test run execution context.</param>
        public virtual void OnBeforeTestRun(TestRunExecutionContext context)
        {
        }

        /// <summary>
        /// The method is called after every test run. Here you can implement all
        /// finalization steps (connections close, rollback, etc).
        /// </summary>
        /// <param name="context">Test run execution context.</param>
        public virtual void OnAfterTestRun(TestRunExecutionContext context)
        {
        }

        /// <summary>
        /// The method is called after all test were done.
        /// </summary>
        /// <param name="context">Test run execution context.</param>
        public virtual void OnShutdown(TestRunExecutionContext context)
        {
        }

        /// <summary>
        /// Run step of test run.
        /// </summary>
        public virtual TestRunResult Run(ITestRunLoader loader, ITestRunLogger logger = null)
        {
            if (loader == null)
            {
                throw new ArgumentNullException(nameof(loader));
            }
            if (logger == null)
            {
                logger = new ConsoleTestRunLogger();
            }

            var context = new TestRunExecutionContext();
            context.StartedAt = DateTime.Now;
            context.Logger = logger;
            context.Resolver = type => null;
            var result = new TestRunResult();

            OnInitialize(context);

            foreach (TestRun testRun in loader.Get())
            {
                context.TestRun = testRun;
                OnBeforeTestRun(context);
                try
                {
                    int stepNumber = 1;
                    foreach (ITestRunStep testRunStep in testRun.Steps)
                    {
                        context.Step = testRunStep;
                        context.StepNumber = stepNumber;
                        try
                        {
                            testRunStep.Run(context);
                        }
                        catch (TestRunAssertException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            if (context.FirstException == null)
                            {
                                context.FirstException = ex;
                            }
                            else
                            {
                                context.LastException = ex;
                            }
                        }
                        stepNumber++;
                    }
                }
                finally
                {
                    context.Step = null;
                    OnAfterTestRun(context);
                }
            }

            OnShutdown(context);

            return result;
        }
    }
}
