// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Saritasa.Tools.Messages.Internal.Clauses;
using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// The SELECT statement builder interface.
    /// </summary>
    internal interface ISelectStringBuilder
    {
        /// <summary>
        /// Gets or sets the selected table.
        /// </summary>
        string SelectedTable { get; }

        /// <summary>
        /// Gets the WHERE statement.
        /// </summary>
        /// <value>
        /// The WHERE statement.
        /// </value>
        IList<WhereClause> WhereStatement { get; }

        /// <summary>
        /// Gets the ORDER BY statement.
        /// </summary>
        /// <value>
        /// The ORDER BY statement.
        /// </value>
        IList<OrderByClause> OrderByStatement { get; }

        /// <summary>
        /// Gets or sets the skipped rows count.
        /// </summary>
        /// <value>
        /// The skipped rows count.
        /// </value>
        int? SkipRows { get; set; }

        /// <summary>
        /// Gets or sets the taken rows count.
        /// </summary>
        /// <value>
        /// The taken rows count.
        /// </value>
        int? TakeRows { get; set; }

        /// <summary>
        /// Selects all columns.
        /// </summary>
        /// <returns>Select string builder.</returns>
        ISelectStringBuilder SelectAll();

        /// <summary>
        /// Sets the specified table names.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <returns>Select string builder.</returns>
        ISelectStringBuilder From(string tableName);

        /// <summary>
        /// Sets WHERE statement for the specified column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>Where clause.</returns>
        WhereClause Where(string columnName);

        /// <summary>
        /// Sets WHERE statement for the specified column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="value">The value.</param>
        /// <returns>Select string builder.</returns>
        ISelectStringBuilder Where(string columnName, ComparisonOperator @operator, object value);

        /// <summary>
        /// Orders result.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>Select string builder.</returns>
        ISelectStringBuilder OrderBy(string columnName);

        /// <summary>
        /// Orders result.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="operator">The operator.</param>
        /// <returns>Select string builder.</returns>
        ISelectStringBuilder OrderBy(string columnName, SortingOperator @operator);

        /// <summary>
        /// Skips the specified rows count.
        /// </summary>
        /// <param name="rows">The rows count.</param>
        /// <returns>Select string builder.</returns>
        ISelectStringBuilder Skip(int rows);

        /// <summary>
        /// Takes the specified rows count.
        /// </summary>
        /// <param name="rows">The rows count.</param>
        /// <returns>Select string builder.</returns>
        ISelectStringBuilder Take(int rows);

        /// <summary>
        /// Builds the SELECT statement.
        /// </summary>
        /// <returns>The SELECT statement string.</returns>
        string Build();
    }
}
