// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run runner. User should implement this class to perform necessary
    /// operations for every test run.
    /// </summary>
    public abstract class TestRunRunner : IDisposable
    {
        private bool isInitialized;

        /// <summary>
        /// Service provider factory.
        /// </summary>
        public IServiceProviderFactory ServiceProviderFactory { get; set; }

        /// <summary>
        /// Has test runner been initialized.
        /// </summary>
        public virtual bool IsInitialized => isInitialized;

        /// <summary>
        /// Synchronization object.
        /// </summary>
        public object SyncRoot { get; } = new object();

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
        /// <param name="context">Test run execution context. It may be null if called from
        /// Dispose method.</param>
        public virtual void OnShutdown(TestRunExecutionContext context)
        {
        }

        /// <summary>
        /// Run test run with assert method. OnAfterTestRun will be called afterwards.
        /// </summary>
        /// <param name="testRun">Test run.</param>
        /// <param name="assert">Assert method.</param>
        /// <param name="logger">Logger. Console is used by default.</param>
        /// <returns>Test run result.</returns>
        public virtual TestRunResult RunWithAssert(
            TestRun testRun,
            Action<TestRun, TestRunResult> assert,
            ITestRunLogger logger = null)
        {
            TestRunExecutionContext context = null;
            try
            {
                var result = RunInternal(testRun, logger, out context);
                context.Logger.LogWithTime("Run asserts.");
                assert(testRun, result);
                return result;
            }
            finally
            {
                context?.Logger.LogWithTime("Run OnAfterTestRun.");
                OnAfterTestRun(context);
            }
        }

        /// <summary>
        /// Run test run.
        /// </summary>
        /// <param name="testRun">Test run.</param>
        /// <param name="logger">Logger. Console is used by default.</param>
        /// <returns>Test run result.</returns>
        public virtual TestRunResult Run(TestRun testRun, ITestRunLogger logger = null)
        {
            TestRunExecutionContext context = null;
            try
            {
                return RunInternal(testRun, logger, out context);
            }
            finally
            {
                context?.Logger.LogWithTime("Run OnAfterTestRun.");
                OnAfterTestRun(context);
            }
        }

        /// <summary>
        /// Run multiple test runs.
        /// </summary>
        /// <param name="testRuns">Test runs list.</param>
        /// <param name="logger">Logger to be used.</param>
        /// <returns>Results.</returns>
        public virtual IList<TestRunResult> Run(IList<TestRun> testRuns, ITestRunLogger logger = null)
        {
            if (testRuns == null)
            {
                throw new ArgumentNullException(nameof(testRuns));
            }
            if (logger == null)
            {
                logger = new ConsoleTestRunLogger();
            }

            var results = new List<TestRunResult>(testRuns.Count);
            foreach (TestRun testRun in testRuns)
            {
                Run(testRun, logger);
            }
            return results;
        }

        /// <summary>
        /// Run multiple test runs from test run loader.
        /// </summary>
        /// <param name="testRunLoader">Test run loader.</param>
        /// <param name="logger">Logger to be used.</param>
        /// <returns>Results.</returns>
        public virtual IList<TestRunResult> Run(ITestRunLoader testRunLoader, ITestRunLogger logger = null)
        {
            if (testRunLoader == null)
            {
                throw new ArgumentNullException(nameof(testRunLoader));
            }
            if (logger == null)
            {
                logger = new ConsoleTestRunLogger();
            }

            var results = new List<TestRunResult>();
            foreach (TestRun testRun in testRunLoader.Get())
            {
                Run(testRun, logger);
            }
            return results;
        }

        private void Initialize(TestRunExecutionContext context)
        {
            if (!isInitialized)
            {
                lock (SyncRoot)
                {
                    if (!isInitialized)
                    {
                        context.Logger.LogWithTime("Run initialization.");
                        OnInitialize(context);
                        isInitialized = true;
                    }
                }
            }
        }

        private TestRunResult RunInternal(TestRun testRun, ITestRunLogger logger,
            out TestRunExecutionContext context)
        {
            if (testRun == null)
            {
                throw new ArgumentNullException(nameof(testRun));
            }
            if (logger == null)
            {
                logger = new ConsoleTestRunLogger();
            }

            logger.LogWithTime($"Started to run \"{testRun.Name}\" test.");
            context = new TestRunExecutionContext
            {
                StartedAt = DateTime.Now,
                Logger = logger
            };

            Initialize(context);

            context.Logger.LogWithTime("Run OnBeforeTestRun.");
            OnBeforeTestRun(context);
            var result = new TestRunResult(testRun.Name);
            context.Logger.LogWithTime("Run Test.");
            InvokeTestRun(testRun, context, result, logger);

            return result;
        }

        private void InvokeTestRun(TestRun testRun, TestRunExecutionContext context, TestRunResult result,
            ITestRunLogger logger)
        {
            context.TestRun = testRun;
            context.ServiceProvider = ServiceProviderFactory.Create();
            OnBeforeTestRun(context);
            try
            {
                int stepNumber = 1;
                foreach (ITestRunStep testRunStep in testRun.Steps)
                {
                    context.StepNumber = stepNumber++;
                    InvokeStep(testRunStep, context, result, logger);
                }
            }
            finally
            {
                context.Step = null;
                var disposableServiceProvider = context.ServiceProvider as IDisposable;
                disposableServiceProvider?.Dispose();
                OnAfterTestRun(context);
            }
        }

        private void InvokeStep(ITestRunStep step, TestRunExecutionContext context, TestRunResult result,
            ITestRunLogger logger)
        {
            context.Step = step;
            try
            {
                step.Run(context);
            }
            catch (TestRunAssertException)
            {
                result.AddStep(new TestRunStepResult(context.StepNumber, step.ToString(), false));
                throw;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                if (context.FirstException == null)
                {
                    result.FailException = ex;
                    context.FirstException = ex;
                }
                else
                {
                    context.LastException = ex;
                }
            }
            result.AddStep(new TestRunStepResult(context.StepNumber, step.ToString(), result.IsSuccess));
        }

        #region Dispose

        private bool disposed;

        /// <summary>
        /// Dispose pattern impementation.
        /// </summary>
        /// <param name="disposing">Dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    var disposableServiceProviderFactory = ServiceProviderFactory as IDisposable;
                    disposableServiceProviderFactory?.Dispose();
                }
                OnShutdown(TestRunExecutionContext.Empty);
                disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
