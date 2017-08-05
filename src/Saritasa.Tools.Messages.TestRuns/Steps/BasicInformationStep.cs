// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Saritasa.Tools.Messages.TestRuns.Steps
{
    /// <summary>
    /// Step contains basic information about test run. Does not do anything
    /// exception keeping data to file.
    /// </summary>
    public class BasicInformationStep : ITestRunStep
    {
        private const string KeyName = "name";
        private const string KeyDescription = "description";
        private const string KeyAuthor = "author";
        private const string KeyCreated = "created";
        private const string KeyTags = "tags";

        /// <summary>
        /// Test name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Overall test run description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Test run author.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Tags.
        /// </summary>
        public IList<string> Tags { get; set; }

        /// <summary>
        /// When test run has been created.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <inheritdoc />
        public void Run(TestRunExecutionContext context)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public BasicInformationStep()
        {
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="body">Deserialize from json.</param>
        public BasicInformationStep(JObject body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            var j = body;
            if (j[KeyName] != null)
            {
                Name = j[KeyName].ToString();
            }
            if (j[KeyAuthor] != null)
            {
                Author = j[KeyAuthor].ToString();
            }
            if (j[KeyDescription] != null)
            {
                Description = j[KeyDescription].ToString();
            }
            if (!string.IsNullOrEmpty(j[KeyCreated]?.ToString()))
            {
                CreatedAt = DateTime.Parse(j[KeyCreated].ToString());
            }
            if (j[KeyTags] != null && j[KeyTags].HasValues)
            {
                Tags = j[KeyTags].ToObject<List<string>>();
            }
        }

        /// <inheritdoc />
        public JObject Save()
        {
            var j = new JObject
            {
                [KeyName] = Name,
                [KeyAuthor] = Author,
                [KeyCreated] = CreatedAt,
                [KeyDescription] = Description,
                [KeyTags] = new JArray(Tags)
            };
            return j;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}: (Author: {Author})";
        }
    }
}
