// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Saritasa.Tools.Messages.Repositories.QueryProviders
{
    /// <summary>
    /// Sql query provider for AdoNetMessageRepository.
    /// </summary>
    internal interface IMessageQueryProvider
    {
        /// <summary>
        /// Create initial table for messages.
        /// </summary>
        /// <returns>Sql query.</returns>
        string GetCreateTableScript();

        /// <summary>
        /// Returns script to check whether messages table exists.
        /// </summary>
        /// <returns>Sql query.</returns>
        string GetExistsTableScript();

        /// <summary>
        /// Returns sql script to insert message.
        /// </summary>
        /// <returns>Sql query.</returns>
        string GetInsertMessageScript();

        /// <summary>
        /// Get filter script.
        /// </summary>
        /// <param name="messageQuery">Message query to filter by.</param>
        /// <returns>Sql query.</returns>
        string GetFilterScript(MessageQuery messageQuery);
    }
}
