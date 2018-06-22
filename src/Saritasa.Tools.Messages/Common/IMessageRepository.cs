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
    /// Message repository interface.
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>
        /// Add message.
        /// </summary>
        /// <param name="messageRecord">Message record.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        Task AddAsync(MessageRecord messageRecord, CancellationToken cancellationToken);

        /// <summary>
        /// Get messages filtered by expression.
        /// </summary>
        /// <param name="messageQuery">Query.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Enumerable of message records.</returns>
        Task<IEnumerable<MessageRecord>> GetAsync(MessageQuery messageQuery, CancellationToken cancellationToken);

        /// <summary>
        /// Save internal state to dict. The methpd represents the ability of message repository to
        /// to save its internal state to dictionary. Also type should define static Create
        /// method to recreate itself from dict.
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        void SaveState(IDictionary<string, string> parameters);
    }
}
