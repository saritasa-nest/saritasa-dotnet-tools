// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// Discards messages. Used mainly for debugging and benchmarking.
    /// </summary>
    public class NullMessageRepository : IMessageRepository
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public NullMessageRepository()
        {
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        public NullMessageRepository(IDictionary<string, string> dict)
        {
        }

        #region IMessageRepository

        static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public Task AddAsync(IMessage message)
        {
            // No need to implement since repository should not persist messages.
            return completedTask;
        }

        /// <inheritdoc />
        public Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery)
        {
            return Task.FromResult(new List<Message>().Cast<IMessage>());
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, string> dict)
        {
            // No need to implement since repository does not have state.
        }

        #endregion
    }
}
