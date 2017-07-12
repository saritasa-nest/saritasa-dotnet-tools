// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// Simple in memory message repository.
    /// </summary>
    public class InMemoryMessageRepository : IMessageRepository
    {
        /// <summary>
        /// All stored messages.
        /// </summary>
        public IList<IMessage> Messages { get; }

        private readonly object objLock = new object();

        #region IMessageRepository

        private static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <summary>
        /// .ctor
        /// </summary>
        public InMemoryMessageRepository()
        {
            Messages = new List<IMessage>();
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        public InMemoryMessageRepository(IDictionary<string, string> parameters) : this()
        {
        }

        /// <inheritdoc />
        public Task AddAsync(IMessage message, CancellationToken cancellationToken)
        {
            lock (objLock)
            {
                Messages.Add(message);
            }
            return completedTask;
        }

        /// <inheritdoc />
        public Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery, CancellationToken cancellationToken)
        {
            lock (objLock)
            {
                return Task.FromResult(Messages.Where(messageQuery.Match));
            }
        }

        #endregion

        /// <inheritdoc />
        public void SaveState(IDictionary<string, string> parameters)
        {
            // No need to implement since repository does not have state.
        }

        /// <summary>
        /// Dump messages into string.
        /// </summary>
        /// <returns>String with data.</returns>
        public string Dump()
        {
            var sb = new StringBuilder((Messages.Count + 1) * 250);
            foreach (var message in Messages)
            {
                sb.AppendLine(JsonConvert.SerializeObject(message, Formatting.Indented));
                sb.AppendLine("----------");
            }
            return sb.ToString();
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
