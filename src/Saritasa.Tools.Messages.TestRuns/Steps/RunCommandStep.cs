// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.TestRuns.Internal;

namespace Saritasa.Tools.Messages.TestRuns.Steps
{
    /// <summary>
    /// Run command within command pipeline. To run requires <see cref="ICommandPipeline" />.
    /// </summary>
    public class RunCommandStep : ITestRunStep
    {
        private const string KeyId = "id";
        private const string KeyType = "type";
        private const string KeyContent = "content";

        private readonly Guid id;

        private readonly string commandType;

        private readonly object command;

        private readonly bool typeWasResolved;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="id">Command id.</param>
        /// <param name="command">Command object.</param>
        public RunCommandStep(Guid id, object command)
        {
            this.id = id;
            this.commandType = command.GetType().AssemblyQualifiedName;
            this.command = command;
        }

        /// <summary>
        /// .ctor to create from <see cref="IMessageContext" />.
        /// </summary>
        /// <param name="message">Message.</param>
        public RunCommandStep(IMessageContext message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            this.id = message.Id;
            this.commandType = message.ContentId;
            this.command = message.Content;
        }

        /// <summary>
        /// .ctor to create from string line.
        /// </summary>
        /// <param name="body">String.</param>
        public RunCommandStep(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            var j = JObject.Parse(body);
            this.id = Guid.Parse(j[KeyId].ToString());
            this.commandType = j[KeyType].ToString();
            var type = TypeHelpers.LoadType(this.commandType);
            if (type != null)
            {
                this.typeWasResolved = true;
            }
            this.command = JsonConvert.DeserializeObject(j[KeyContent].ToString(), type);
        }

        /// <inheritdoc />
        public void Run(TestRunExecutionContext context)
        {
            /*if (!typeWasResolved)
            {
                throw new TestRunException($"Cannot load type {this.commandType} for id {this.id}.");
            }
            var commandPipeline = context.Resolver(typeof(ICommandPipeline)) as ICommandPipeline;
            if (commandPipeline == null)
            {
                throw new TestRunException($"Cannot resolve {nameof(ICommandPipeline)}.");
            }
            commandPipeline.Handle(command);
            context.LastResult = command;*/
        }

        /// <inheritdoc />
        public string Save()
        {
            var j = new JObject
            {
                [KeyId] = id,
                [KeyType] = commandType,
                [KeyContent] = JObject.FromObject(command)
            };
            return JsonConvert.SerializeObject(j, Formatting.Indented);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{commandType} <{id}>";
        }
    }
}
