// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run runner. User should implement this class to perform necessary
    /// operations for every test run.
    /// </summary>
    public abstract class TestRunRunner : IDisposable
    {
        /// <summary>
        /// Service provider factory.
        /// </summary>
        public IServiceProviderFactory ServiceProviderFactory { get; set; }

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

            var context = new TestRunExecutionContext
            {
                StartedAt = DateTime.Now,
                Logger = logger
            };

            OnInitialize(context);

            var result = new TestRunResult();
            foreach (TestRun testRun in loader.Get())
            {
                InvokeTestRun(testRun, context, result, logger);
            }

            OnShutdown(context);

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
