// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Repositories
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Discards messages. Used mainly for debugging and benchmarking.
    /// </summary>
    public class NullMessageRepository : IMessageRepository
    {
        #region IMessageRepository

        /// <inheritdoc />
        public void Add(Message message)
        {
        }

        /// <inheritdoc />
        public IEnumerable<Message> Get(MessageQuery messageQuery)
        {
            return new List<Message>();
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, object> dict)
        {
        }

        #endregion

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        /// <returns>Message repository.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            Justification = "Need only for general use", MessageId = "dict")]
        public static IMessageRepository CreateFromState(IDictionary<string, object> dict)
        {
            return new NullMessageRepository();
        }
    }
}
