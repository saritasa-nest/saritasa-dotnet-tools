// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run step result.
    /// </summary>
    public class TestRunStepResult
    {
        /// <summary>
        /// Step number.
        /// </summary>
        public int StepNum { get; }

        /// <summary>
        /// Test run step name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Was test step run succeed.
        /// </summary>
        public bool IsSuccess { get; } = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stepNum">Step number.</param>
        /// <param name="name">Step name.</param>
        /// <param name="isSuccess">Was test step run succeed.</param>
        public TestRunStepResult(int stepNum, string name, bool isSuccess)
        {
            this.StepNum = stepNum;
            this.Name = name;
            this.IsSuccess = isSuccess;
        }
    }
}
