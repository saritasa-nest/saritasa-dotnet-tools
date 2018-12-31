// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// The repository discards messages. To be used for debugging and benchmarking.
    /// </summary>
    public class NullMessageRepository : IMessageRepository
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public NullMessageRepository()
        {
        }

        /// <summary>
        /// Creates repository from dictionary.
        /// </summary>
        /// <param name="parameters">Properties.</param>
        public NullMessageRepository(IDictionary<string, string> parameters)
        {
        }

        #region IMessageRepository

        private static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public Task AddAsync(MessageRecord message, CancellationToken cancellationToken = default(CancellationToken))
        {
            // No need to implement since repository should not persist messages.
            return completedTask;
        }

        /// <inheritdoc />
        public Task<IEnumerable<MessageRecord>> GetAsync(MessageQuery messageQuery, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<MessageRecord>().AsEnumerable());
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, string> parameters)
        {
            // No need to implement since repository does not have state.
        }

        #endregion
    }
}
