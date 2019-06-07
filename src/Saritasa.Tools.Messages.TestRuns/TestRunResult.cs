// Copyright (c) 2017-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Saritasa.Tools.Messages.TestRuns.Steps;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run results.
    /// </summary>
    public class TestRunResult
    {
        /// <summary>
        /// Was test run succeed.
        /// </summary>
        public bool IsSuccess { get; internal set; } = true;

        /// <summary>
        /// Test run name. Getting from <see cref="BasicInformationStep" /> step.
        /// </summary>
        public string Name { get; internal set; } = "(none)";

        /// <summary>
        /// Exception if any occured.
        /// </summary>
        public Exception FailException { get; internal set; }

        /// <summary>
        /// Fail test step number where <see cref="FailException" /> occurred. -1 by default.
        /// </summary>
        public int FailStepNumber { get; internal set; } = -1;

        private readonly IList<TestRunStepResult> steps = new List<TestRunStepResult>();

        /// <summary>
        /// Test run steps results.
        /// </summary>
        public IEnumerable<TestRunStepResult> Steps => steps;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TestRunResult()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Test run name.</param>
        public TestRunResult(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }
        }

        /// <summary>
        /// Add step result.
        /// </summary>
        /// <param name="step">Step result.</param>
        public void AddStep(TestRunStepResult step)
        {
            steps.Add(step);
        }

        /// <summary>
        /// Dump result state to logger.
        /// </summary>
        /// <param name="logger">Logger to be used.</param>
        public void Dump(ITestRunLogger logger)
        {
#pragma warning disable SA1001 // Commas should be spaced correctly
            logger.Log(new string('-', 96));
            logger.Log($"{TruncateString(Name, 90), -90}{GetSuccessString(IsSuccess), 6}");
            foreach (TestRunStepResult step in Steps)
            {
                logger.Log(
                    $"{"#" + step.StepNum, -4}{TruncateString(step.Name, 86), -86}{GetSuccessString(step.IsSuccess), 6}");
            }
            logger.Log(new string('-', 96));
#pragma warning restore SA1001 // Commas should be spaced correctly
        }

        /// <summary>
        /// Throw <see cref="TestRunException" /> if test run was not succeed.
        /// </summary>
        public void Assert()
        {
            if (!IsSuccess)
            {
                if (FailException != null)
                {
                    throw new TestRunException(
                        $"Test run \"{Name}\" execution failed on step {FailStepNumber}.", FailException);
                }
                else
                {
                    throw new TestRunException($"Test run \"{Name}\" execution failed.");
                }
            }
        }

        private static string TruncateString(string target, int maxLength)
        {
            if (string.IsNullOrEmpty(target))
            {
                return target;
            }
            return target.Length <= maxLength ? target : target.Substring(0, maxLength);
        }

        internal static string GetSuccessString(bool isSuccess)
        {
            return isSuccess ? "PASS" : "FAIL";
        }
    }
}
