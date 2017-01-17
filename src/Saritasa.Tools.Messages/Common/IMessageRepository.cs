// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Abstractions;

    /// <summary>
    /// Message repository interface.
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>
        /// Add message.
        /// </summary>
        /// <param name="message">Message.</param>
        Task AddAsync([NotNull] IMessage message);

        /// <summary>
        /// Get messages filtered by expression.
        /// </summary>
        /// <param name="messageQuery">Query.</param>
        /// <returns>Enumerable of messages.</returns>
        Task<IEnumerable<IMessage>> GetAsync([NotNull] MessageQuery messageQuery);

        /// <summary>
        /// Save internal state to dict. The methpd represents the ability of message repository to
        /// to save its internal state to dictionary. Also type should define static Create
        /// method to recreate itself from dict.
        /// </summary>
        /// <param name="dict">Dictionary of properties.</param>
        void SaveState([NotNull] IDictionary<string, object> dict);
    }
}
