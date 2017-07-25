// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json.Linq;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run step. Represent the action to be executed. Also step should have ability
    /// to be saved into string.
    /// </summary>
    public interface ITestRunStep
    {
        /// <summary>
        /// Run step.
        /// </summary>
        /// <param name="context">Test run execution context.</param>
        void Run(TestRunExecutionContext context);

        /// <summary>
        /// Save test run step as string.
        /// </summary>
        /// <returns>Serialized presentation of step state.</returns>
        JObject Save();
    }
}
