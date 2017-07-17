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
        /// <param name="body">Deserialize from lines.</param>
        public BasicInformationStep(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            var j = JObject.Parse(body);
            if (j[KeyName] != null && j[KeyName].HasValues)
            {
                Name = j[KeyName].ToString();
            }
            if (j[KeyAuthor] != null && j[KeyAuthor].HasValues)
            {
                Author = j[KeyAuthor].ToString();
            }
            if (j[KeyDescription] != null && j[KeyDescription].HasValues)
            {
                Description = j[KeyDescription].ToString();
            }
            if (j[KeyCreated] != null && j[KeyCreated].HasValues)
            {
                CreatedAt = DateTime.Parse(j[KeyCreated].ToString());
            }
            if (j[KeyTags] != null && j[KeyTags].HasValues)
            {
                Tags = new JArray(j[KeyTags]).Select(t => t.First.ToString()).ToList();
            }
        }

        /// <inheritdoc />
        public string Save()
        {
            var j = new JObject
            {
                [KeyName] = Name,
                [KeyAuthor] = Author,
                [KeyCreated] = CreatedAt,
                [KeyDescription] = Description,
                [KeyTags] = new JArray(Tags)
            };
            return JsonConvert.SerializeObject(j, Formatting.Indented);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}: (Author: {Author})";
        }
    }
}
