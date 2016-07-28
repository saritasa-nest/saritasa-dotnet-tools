// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Simple in memory message repository.
    /// </summary>
    public class InMemoryMessageRepository : IMessageRepository
    {
        /// <summary>
        /// All stored messages.
        /// </summary>
        public IList<Message> Messages { get; private set; }

        private readonly object objLock = new object();

        /// <summary>
        /// .ctor
        /// </summary>
        public InMemoryMessageRepository()
        {
            Messages = new List<Message>();
        }

        /// <inheritdoc />
        public void Add(Message message)
        {
            lock (objLock)
            {
                Messages.Add(message);
            }
        }

        /// <inheritdoc />
        public IEnumerable<Message> Get(MessageQuery messageQuery)
        {
            var func = messageQuery.MessageSelector.Compile();
            lock (objLock)
            {
                return Messages.Where(func).ToList();
            }
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, object> dict)
        {
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        /// <returns>Message repository.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            Justification = "Need only for general purpose", MessageId = "dict")]
        public static IMessageRepository CreateFromState(IDictionary<string, object> dict)
        {
            return new InMemoryMessageRepository();
        }
    }
}
