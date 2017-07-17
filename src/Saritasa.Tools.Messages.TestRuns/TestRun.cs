// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Saritasa.Tools.Messages.TestRuns.Steps;

namespace Saritasa.Tools.Messages.TestRuns
{
    /// <summary>
    /// Test run. Wraps series of steps to be executed for test run.
    /// </summary>
    public class TestRun
    {
        private const int Version = 1;

        private string FormatDelimiterLine(int stepNumber, string target) => $"/*>! #{stepNumber} {target} */";

        private string FormatHeaderLine(int version) => $"/*>! V{version} Saritasa Tools Messages TestRun file. */";

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
            streamWriter.WriteLine(FormatHeaderLine(Version));
            for (int i = 0; i < Steps.Count; i++)
            {
                var step = Steps[i];
                var stepTypeName = step.GetType().Name;
                streamWriter.WriteLine(FormatDelimiterLine(i + 1, stepTypeName));
                streamWriter.WriteLine(step.Save());
                streamWriter.WriteLine();
            }
        }

        /// <summary>
        /// Load test run data from stream.
        /// </summary>
        /// <param name="reader">Stream reader.</param>
        /// <returns>Test run.</returns>
        public static TestRun Load(StreamReader reader)
        {
            var sb = new StringBuilder();
            var testRun = new TestRun();
            string currentStepTypeName = string.Empty;

            var versionLine = GetTypeNameFromDelimeterLine(reader.ReadLine());
            var version = GetVersionFromName(versionLine);
            if (version != Version)
            {
                throw new TestRunException("Incorrect version file format.");
            }

            while (!reader.EndOfStream)
            {
                var line = (reader.ReadLine() ?? string.Empty).Trim();
                if (line.StartsWith("//"))
                {
                    continue;
                }

                if (IsDelimeterLine(line))
                {
                    if (!string.IsNullOrEmpty(currentStepTypeName))
                    {
                        var step = CreateTestRunStepFromTypeName(currentStepTypeName, sb.ToString());
                        testRun.Steps.Add(step);
                        sb.Clear();
                    }
                    currentStepTypeName = GetTypeNameFromDelimeterLine(line);
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            if (!string.IsNullOrEmpty(currentStepTypeName))
            {
                var step = CreateTestRunStepFromTypeName(currentStepTypeName, sb.ToString());
                testRun.Steps.Add(step);
            }

            if (!testRun.Steps.Any() && string.IsNullOrEmpty(currentStepTypeName))
            {
                throw new TestRunException("Invalid stream format.");
            }
            if (!testRun.Steps.Any(s => s is BasicInformationStep))
            {
                throw new TestRunException($"Stream must contain at least one {nameof(BasicInformationStep)} step.");
            }

            return testRun;
        }

        private static int GetVersionFromName(string line)
        {
            if (line.StartsWith("V") && line.Length > 1)
            {
                var v = line.Substring(1);
                if (int.TryParse(v, out int versionInt))
                {
                    return versionInt;
                }
            }
            throw new TestRunException("Incorrect file header.");
        }

        private static bool IsDelimeterLine(string line) => line.StartsWith("/*>!") && line.EndsWith("*/");

        /// <summary>
        /// Possible formates are: #X Type, Type
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <returns>Type name or empty string.</returns>
        private static string GetTypeNameFromDelimeterLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }
            line = line.Replace("/*>!", string.Empty).Replace("*/", string.Empty).Trim();
            var arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length > 1 && arr[0].Length > 0 && arr[0][0] == '#')
            {
                return arr[1];
            }
            if (arr.Length > 0)
            {
                return arr[0];
            }
            return string.Empty;
        }

        /// <summary>
        /// Create test run step from type name.
        /// </summary>
        /// <param name="typeName">Partial or full qualified type name.</param>
        /// <param name="body">Initialize from text. Optional.</param>
        /// <returns>New test run step.</returns>
        public static ITestRunStep CreateTestRunStepFromTypeName(string typeName, string body = null)
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
                && c.GetParameters().First().ParameterType == typeof(string));
            if (ctor == null)
            {
                ctor = type.GetTypeInfo().GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
            }

            if (ctor == null)
            {
                throw new TestRunException($"Type {typeName} does not have ctor with string parameter or parameterless ctor.");
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
