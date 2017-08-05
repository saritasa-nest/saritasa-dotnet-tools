// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Saritasa.Tools.Messages.TestRuns.Steps;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run. Wraps series of steps to be executed for test run.
    /// </summary>
    public class TestRun
    {
        /// <summary>
        /// Json field name for step number.
        /// </summary>
        public const string NumberKey = "number";

        /// <summary>
        /// Json field name for type.
        /// </summary>
        public const string TypeKey = "type";

        /// <summary>
        /// Json field name for content.
        /// </summary>
        public const string ContentKey = "content";

        /// <summary>
        /// Test run steps.
        /// </summary>
        public IList<ITestRunStep> Steps { get; protected set; } = new List<ITestRunStep>();

        /// <summary>
        /// .ctor
        /// </summary>
        internal TestRun()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="name">Test name</param>
        public TestRun(string name)
        {
            Steps.Add(new BasicInformationStep
            {
                Name = name
            });
        }

        /// <summary>
        /// Test name.
        /// </summary>
        public string Name
        {
            get => GetBasicInformation().Name;
            set => GetBasicInformation().Name = value;
        }

        /// <summary>
        /// Overall description what test does and what it tests.
        /// </summary>
        public string Description
        {
            get => GetBasicInformation().Description;
            set => GetBasicInformation().Description = value;
        }

        /// <summary>
        /// Author of the test.
        /// </summary>
        public string Author
        {
            get => GetBasicInformation().Author;
            set => GetBasicInformation().Author = value;
        }

        /// <summary>
        /// Tags related to test.
        /// </summary>
        public IList<string> Tags
        {
            get => GetBasicInformation().Tags;
            set => GetBasicInformation().Tags = value;
        }

        /// <summary>
        /// When test run first has been created.
        /// </summary>
        public DateTime? CreatedAt
        {
            get => GetBasicInformation().CreatedAt;
            set => GetBasicInformation().CreatedAt = value;
        }

        /// <summary>
        /// Save test run to stream.
        /// </summary>
        /// <param name="streamWriter">Stream writer.</param>
        public void Save(StreamWriter streamWriter)
        {
            var jarr = new JArray();
            for (int i = 0; i < Steps.Count; i++)
            {
                var step = Steps[i];
                var jstep = new JObject
                {
                    [NumberKey] = "#" + i,
                    [TypeKey] = step.GetType().Name,
                    [ContentKey] = step.Save()
                };
                jarr.Add(jstep);
            }
            streamWriter.WriteLine(jarr.ToString(Formatting.Indented));
        }

        /// <summary>
        /// Empty test run.
        /// </summary>
        public static readonly TestRun Empty = new TestRun("Empty.")
        {
            Description = "Test run without steps only for test purposes."
        };

        /// <summary>
        /// Load test run data from stream.
        /// </summary>
        /// <param name="reader">Stream reader.</param>
        /// <returns>Test run.</returns>
        public static TestRun Load(StreamReader reader)
        {
            var testRun = new TestRun();

            using (var jsonReader = new JsonTextReader(reader))
            {
                var jarr = JToken.ReadFrom(jsonReader);
                foreach (JToken jToken in jarr)
                {
                    var type = jToken[TypeKey].ToString();
                    var content = jToken[ContentKey] as JObject;
                    if (string.IsNullOrEmpty(type) || content == null)
                    {
                        throw new InvalidOperationException($"{TypeKey} or {ContentKey} are not specified.");
                    }
                    var step = CreateTestRunStepFromTypeName(type, content);
                    testRun.Steps.Add(step);
                }
            }

            if (!testRun.Steps.Any())
            {
                throw new TestRunException("Invalid stream format.");
            }
            if (!testRun.Steps.Any(s => s is BasicInformationStep))
            {
                throw new TestRunException($"Stream must contain at least one {nameof(BasicInformationStep)} step.");
            }

            return testRun;
        }

        /// <summary>
        /// Create test run step from type name.
        /// </summary>
        /// <param name="typeName">Partial or full qualified type name.</param>
        /// <param name="body">Initialize from json. Optional.</param>
        /// <returns>New test run step.</returns>
        public static ITestRunStep CreateTestRunStepFromTypeName(string typeName, JObject body = null)
        {
            var type = Type.GetType("Saritasa.Tools.Messages.TestRuns.Steps." + typeName);
            if (type == null)
            {
                type = Type.GetType(typeName);
            }
            if (type == null)
            {
                throw new TestRunException($"Cannot load TestRunStep type {typeName}.");
            }

            var ctor = type.GetTypeInfo().GetConstructors().FirstOrDefault(
                c => c.GetParameters().Length == 1
                && c.GetParameters().First().ParameterType == typeof(JObject));
            if (ctor == null)
            {
                ctor = type.GetTypeInfo().GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
            }

            if (ctor == null)
            {
                throw new TestRunException($"Type {typeName} does not have ctor with JObject parameter or parameterless ctor.");
            }
            return ctor.GetParameters().Length > 0
                ? (ITestRunStep)ctor.Invoke(new object[] { body })
                : (ITestRunStep)ctor.Invoke(new object[] { });
        }

        private BasicInformationStep GetBasicInformation()
        {
            var step = (BasicInformationStep)Steps.FirstOrDefault(s => s is BasicInformationStep);
            if (step == null)
            {
                throw new TestRunException($"{nameof(BasicInformationStep)} not found.");
            }
            return step;
        }
    }
}
