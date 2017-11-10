// Copyright (c) 2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json.Linq;

namespace Saritasa.Tools.Messages.TestRuns.Steps
{
    /// <summary>
    /// The step is to provide comment text within steps list
    /// </summary>
    public class CommentStep : ITestRunStep
    {
        private const string KeyComment = "comment";

        /// <summary>
        /// Comment text.
        /// </summary>
        public string Comment { get; set; }

        /// <inheritdoc />
        public void Run(TestRunExecutionContext context)
        {
        }

        /// <inheritdoc />
        public JObject Save()
        {
            var j = new JObject
            {
                [KeyComment] = Comment
            };
            return j;
        }
    }
}
