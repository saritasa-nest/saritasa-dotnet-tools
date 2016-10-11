// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace Saritasa.Tools.Messages.PipelineMiddlewares
{
    using System;
    using System.Collections.Generic;
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

        /// <summary>
        /// Only messages with specified statuses will be stored.
        /// </summary>
        public Message.ProcessingStatus[] Statuses { get; private set; }

        /// <summary>
        /// Only messages with execution time above will be stored.
        /// </summary>
        public int ExecutionDurationAbove { get; private set; }

        /// <summary>
        /// Only messages that matches regexp selected content type pattern will be stored.
        /// </summary>
        public Regex[] IncludeContentTypes { get; private set; }

        /// <summary>
        /// Messages with matched content types will be excluded.
        /// </summary>
        public Regex[] ExcludedContentTypes { get; private set; }

        /// <summary>
        /// Only specified messages types will be stored.
        /// </summary>
        public byte[] Types { get; private set; }

        /// <summary>
        /// Returns true if message matches current filter criterias.
        /// </summary>
        /// <param name="message">Message to match.</param>
        /// <returns>True if matches, otherwise false.</returns>
        public bool IsMatch(Message message)
        {
            if (Statuses?.Contains(message.Status) == false)
            {
                return false;
            }
            if (message.ExecutionDuration < ExecutionDurationAbove)
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
            int length = Statuses?.Length ?? 0;
            var newStatuses = new Message.ProcessingStatus[length + 1];
            Statuses?.CopyTo(newStatuses, 0);
            newStatuses[newStatuses.Length - 1] = status;
            Statuses = newStatuses;
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
            // TODO:
            return this;
        }

        /// <summary>
        /// Exclude specific content types by pattern.
        /// </summary>
        /// <param name="regex">Regular expression.</param>
        /// <returns>Current repository messages filter.</returns>
        public RepositoryMessagesFilter WithExcludeContentType(Regex regex)
        {
            // TODO:
            return this;
        }
    }
}
