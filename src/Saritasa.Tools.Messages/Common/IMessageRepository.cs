// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Message repository.
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>
        /// Adds message.
        /// </summary>
        /// <param name="messageRecord">Message record.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        Task AddAsync(MessageRecord messageRecord, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets messages filtered by expression.
        /// </summary>
        /// <param name="messageQuery">Query.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Enumerable of message records.</returns>
        Task<IEnumerable<MessageRecord>> GetAsync(MessageQuery messageQuery, CancellationToken cancellationToken
             = default(CancellationToken));

        /// <summary>
        /// Saves internal state to dict. The method represents the ability of message repository to
        /// to save its internal state into dictionary. The type should define static Create
        /// method to restore itself from dictionary.
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        void SaveState(IDictionary<string, string> parameters);
    }
}
