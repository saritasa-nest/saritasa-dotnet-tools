// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Repository messages filter.
    /// </summary>
    public class RepositoryMessagesFilter
    {
        /// <summary>
        /// Use fluent API instead.
        /// </summary>
        private RepositoryMessagesFilter()
        {
        }

        /// <summary>
        /// Custom predicate that filters incoming messages.
        /// </summary>
        public Predicate<Message> Predicate { get; private set; }

        Message.ProcessingStatus[] statuses;

        /// <summary>
        /// Only messages with specified statuses will be stored.
        /// </summary>
        public Message.ProcessingStatus[] Statuses => statuses;

        /// <summary>
        /// Only messages with execution time above will be stored.
        /// </summary>
        public int ExecutionDurationAbove { get; private set; }

        Regex[] includeContentTypes;

        /// <summary>
        /// Only messages that matches regexp selected content type pattern will be stored.
        /// </summary>
        public Regex[] IncludeContentTypes => includeContentTypes;

        Regex[] excludedContentTypes;

        /// <summary>
        /// Messages with matched content types will be excluded.
        /// </summary>
        public Regex[] ExcludedContentTypes => excludedContentTypes;

        byte[] types;

        /// <summary>
        /// Only specified messages types will be stored.
        /// </summary>
        public byte[] Types => types;

        /// <summary>
        /// Returns true if message matches current filter criterias.
        /// </summary>
        /// <param name="message">Message to match.</param>
        /// <returns>True if matches, otherwise false.</returns>
        public bool IsMatch(Message message)
        {
            if (Predicate?.Invoke(message) == false ||
                Statuses?.Contains(message.Status) == false ||
                message.ExecutionDuration < ExecutionDurationAbove ||
                excludedContentTypes?.Any(ct => ct.IsMatch(message.ContentType)) == true ||
                includeContentTypes?.Any(ct => ct.IsMatch(message.ContentType)) == false ||
                types?.Contains(message.Type) == false)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Initial filter create.
        /// </summary>
        /// <returns>Filter instance.</returns>
        public static RepositoryMessagesFilter Create()
        {
            return new RepositoryMessagesFilter();
        }

        /// <summary>
        /// Filter with predicate.
        /// </summary>
        /// <param name="predicate">Preducate action.</param>
        /// <returns>Current repository messages filter.</returns>
        public RepositoryMessagesFilter WithPredicate(Predicate<Message> predicate)
        {
            Predicate = predicate;
            return this;
        }

        /// <summary>
        /// Filter with status. The field accumulates.
        /// </summary>
        /// <param name="status">Message status.</param>
        /// <returns>Current repository messages filter.</returns>
        public RepositoryMessagesFilter WithStatus(Message.ProcessingStatus status)
        {
            if (statuses == null)
            {
                statuses = new Message.ProcessingStatus[1];
            }
            else
            {
                Array.Resize(ref statuses, statuses.Length + 1);
            }
            statuses[statuses.Length - 1] = status;
            return this;
        }

        /// <summary>
        /// Filter with execution duration.
        /// </summary>
        /// <param name="val">Execution duration milliseconds.</param>
        /// <returns>Current repository messages filter.</returns>
        public RepositoryMessagesFilter WithExecutionDurationAbove(int val)
        {
            if (val < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(val));
            }
            ExecutionDurationAbove = val;
            return this;
        }

        /// <summary>
        /// Include specific content types by pattern.
        /// </summary>
        /// <param name="regex">Regular expression.</param>
        /// <returns>Current repository messages filter.</returns>
        public RepositoryMessagesFilter WithIncludeContentType(Regex regex)
        {
            if (includeContentTypes == null)
            {
                includeContentTypes = new Regex[1];
            }
            else
            {
                Array.Resize(ref includeContentTypes, includeContentTypes.Length + 1);
            }
            includeContentTypes[includeContentTypes.Length - 1] = regex;
            return this;
        }

        /// <summary>
        /// Exclude specific content types by pattern.
        /// </summary>
        /// <param name="regex">Regular expression.</param>
        /// <returns>Current repository messages filter.</returns>
        public RepositoryMessagesFilter WithExcludeContentType(Regex regex)
        {
            if (excludedContentTypes == null)
            {
                excludedContentTypes = new Regex[1];
            }
            else
            {
                Array.Resize(ref excludedContentTypes, excludedContentTypes.Length + 1);
            }
            excludedContentTypes[excludedContentTypes.Length - 1] = regex;
            return this;
        }

        /// <summary>
        /// Include message type (command, query, event).
        /// </summary>
        /// <param name="type">Message type.</param>
        /// <returns>Current repository messages filter.</returns>
        public RepositoryMessagesFilter WithType(byte type)
        {
            if (types == null)
            {
                types = new byte[1];
            }
            else
            {
                Array.Resize(ref types, types.Length + 1);
            }
            types[types.Length - 1] = type;
            return this;
        }
    }
}
