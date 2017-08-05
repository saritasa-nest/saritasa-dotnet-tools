// Copyright (c) 2015-2017, Saritasa. All rights reserved.
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
        public bool IsSuccess { get; set;  } = true;

        /// <summary>
        /// Test run name. Getting from <see cref="BasicInformationStep" /> step.
        /// </summary>
        public string Name { get; set;  } = "(none)";

        /// <summary>
        /// Exception if any occured.
        /// </summary>
        public Exception FailException { get; set;  }

        private readonly IList<TestRunStepResult> steps = new List<TestRunStepResult>();

        /// <summary>
        /// Test run steps results.
        /// </summary>
        public IEnumerable<TestRunStepResult> Steps => steps;

        /// <summary>
        /// .ctor
        /// </summary>
        public TestRunResult()
        {
        }

        /// <summary>
        /// .ctor
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
            logger.Log(new string('-', 96));
            logger.Log(string.Format("{0,-90}{1,6}", TruncateString(Name, 90), GetSuccessString(IsSuccess)));
            foreach (TestRunStepResult step in Steps)
            {
                logger.Log(string.Format("{0,-4}{1,-86}{2,6}", "#" + step.StepNum, TruncateString(step.Name, 86),
                    GetSuccessString(step.IsSuccess)));
            }
            logger.Log(new string('-', 96));
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
