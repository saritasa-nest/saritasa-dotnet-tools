// Copyright (c) 2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
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
        /// Command id.
        /// </summary>
        public Guid CommandId => id;

        /// <summary>
        /// Command content.
        /// </summary>
        public object CommandContent => command;

        /// <summary>
        /// Command type.
        /// </summary>
        public string CommandType => commandType;

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
        /// <param name="messageRecord">Message record.</param>
        public RunCommandStep(MessageRecord messageRecord)
        {
            if (messageRecord == null)
            {
                throw new ArgumentNullException(nameof(messageRecord));
            }

            this.id = messageRecord.Id;
            this.commandType = messageRecord.ContentType;
            this.command = messageRecord.Content;
        }

        /// <summary>
        /// .ctor to create from string line.
        /// </summary>
        /// <param name="body">JSON.</param>
        public RunCommandStep(JObject body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            var j = body;
            this.id = Guid.Parse(j[KeyId].ToString());
            this.commandType = j[KeyType].ToString();
            var type = TypeHelpers.LoadType(this.commandType);
            if (type != null)
            {
                this.typeWasResolved = true;
            }
            this.command = JsonConvert.DeserializeObject(j[KeyContent].ToString(), type, new JsonSerializerSettings
            {
                Error = (sender, args) => { args.ErrorContext.Handled = true; }
            });
        }

        /// <inheritdoc />
        public void Run(TestRunExecutionContext context)
        {
            if (!typeWasResolved)
            {
                throw new TestRunException($"Cannot load type {this.commandType} for id {this.id}.");
            }
            var pipelines = context.ServiceProvider.GetService(typeof(IMessagePipelineContainer))
                as IMessagePipelineContainer;
            if (pipelines == null)
            {
                throw new TestRunException($"Cannot resolve {nameof(IMessagePipelineContainer)}.");
            }
            var commandPipeline = pipelines.Pipelines.FirstOrDefault(p => p is ICommandPipeline);
            if (pipelines == null)
            {
                throw new TestRunException($"Cannot resolve {nameof(ICommandPipeline)}.");
            }
            var pipelineService = new TestRunPipelineService(context.ServiceProvider, commandPipeline);
            pipelineService.HandleCommand(command);
            context.LastResult = command;
        }

        /// <inheritdoc />
        public JObject Save()
        {
            var j = new JObject
            {
                [KeyId] = id,
                [KeyType] = commandType,
                [KeyContent] = JObject.FromObject(command)
            };
            return j;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{commandType} <{id}>";
        }
    }
}
