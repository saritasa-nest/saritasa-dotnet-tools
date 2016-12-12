// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Internal.Clauses
{
    using Enums;

    /// <summary>
    /// Represents a JOIN clause to be used with SELECT statements.
    /// </summary>
    internal struct JoinClause
    {
        /// <summary>
        /// Gets the type of a JOIN clause.
        /// </summary>
        /// <value>
        /// The type of a JOIN clause.
        /// </value>
        public JoinType JoinType { get; }

        /// <summary>
        /// Gets from table.
        /// </summary>
        /// <value>
        /// From table.
        /// </value>
        public string FromTable { get; }

        /// <summary>
        /// Gets from column.
        /// </summary>
        /// <value>
        /// From column.
        /// </value>
        public string FromColumn { get; }

        /// <summary>
        /// Gets the comparison operator.
        /// </summary>
        /// <value>
        /// The comparison operator.
        /// </value>
        public ComparisonOperator ComparisonOperator { get; }

        /// <summary>
        /// Gets to table.
        /// </summary>
        /// <value>
        /// To table.
        /// </value>
        public string ToTable { get; }

        /// <summary>
        /// Gets to column.
        /// </summary>
        /// <value>
        /// To column.
        /// </value>
        public string ToColumn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinClause" /> struct.
        /// </summary>
        /// <param name="join">The join.</param>
        /// <param name="toTableName">Name of to table.</param>
        /// <param name="toColumnName">Name of to column.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="fromTableName">Name of from table.</param>
        /// <param name="fromColumnName">Name of from column.</param>
        public JoinClause(JoinType join, string toTableName, string toColumnName, ComparisonOperator @operator,
            string fromTableName, string fromColumnName)
        {
            JoinType = join;
            FromTable = fromTableName;
            FromColumn = fromColumnName;
            ComparisonOperator = @operator;
            ToTable = toTableName;
            ToColumn = toColumnName;
        }
    }
}
