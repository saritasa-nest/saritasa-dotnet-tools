// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Internal.Clauses
{
    using Enums;

    /// <summary>
    /// Represents a ORDER BY clause to be used with SELECT statements.
    /// </summary>
    internal class OrderByClause
    {
        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        public string ColumnName { get; }

        /// <summary>
        /// Gets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public SortingOperator SortOrder { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByClause"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        public OrderByClause(string column) : this(column, SortingOperator.Ascending)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByClause"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="order">The sort order.</param>
        public OrderByClause(string column, SortingOperator order)
        {
            ColumnName = column;
            SortOrder = order;
        }
    }
}
