// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Common.Repositories.QueryProviders
{
    /// <summary>
    /// SQL query provider for <see cref="AdoNetMessageRepository" />.
    /// </summary>
    internal interface IMessageQueryProvider
    {
        /// <summary>
        /// Create initial table for messages.
        /// </summary>
        /// <returns>SQL query.</returns>
        string GetCreateTableScript();

        /// <summary>
        /// Returns script to check whether messages table exists.
        /// </summary>
        /// <returns>Sql query.</returns>
        string GetExistsTableScript();

        /// <summary>
        /// Returns SQL script to insert message.
        /// </summary>
        /// <returns>SQL query.</returns>
        string GetInsertMessageScript();

        /// <summary>
        /// Get filter script.
        /// </summary>
        /// <param name="messageQuery">Message query to filter by.</param>
        /// <returns>SQL query.</returns>
        string GetFilterScript(MessageQuery messageQuery);
    }
}
