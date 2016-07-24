// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Saritasa.Tools.Messages.Repositories.SqlProviders
{
    /// <summary>
    /// Sql query provider for AdoNetMessageRepository.
    /// </summary>
    internal interface ISqlProvider
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
        /// Get select all script.
        /// </summary>
        /// <returns>Sql query.</returns>
        string GetSelectAllScript();

        /// <summary>
        /// Adds where condition to query.
        /// </summary>
        /// <param name="sb">String builder.</param>
        /// <param name="fieldind">Field index.</param>
        /// <param name="op">Operator.</param>
        /// <param name="value">Value.</param>
        void AddAndWhereCondition(StringBuilder sb, int fieldind, string op, object value);
    }
}
