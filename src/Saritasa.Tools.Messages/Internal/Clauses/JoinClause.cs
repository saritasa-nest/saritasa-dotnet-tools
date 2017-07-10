// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Internal.Clauses
{
    /// <summary>
    /// Represents a JOIN clause to be used with SELECT statements.
    /// </summary>
    internal class JoinClause
    {
        private readonly ISelectStringBuilder builder;

        /// <summary>
        /// Gets the type of a JOIN clause.
        /// </summary>
        /// <value>
        /// The type of a JOIN clause.
        /// </value>
        public JoinType JoinType { get; set; }

        /// <summary>
        /// Gets from table.
        /// </summary>
        /// <value>
        /// From table.
        /// </value>
        public string FromTable { get; set; }

        /// <summary>
        /// Gets from column.
        /// </summary>
        /// <value>
        /// From column.
        /// </value>
        public string FromColumn { get; set; }

        /// <summary>
        /// Gets the comparison operator.
        /// </summary>
        /// <value>
        /// The comparison operator.
        /// </value>
        public ComparisonOperator ComparisonOperator { get; set; }

        /// <summary>
        /// Gets to table.
        /// </summary>
        /// <value>
        /// To table.
        /// </value>
        public string ToTable { get; set; }

        /// <summary>
        /// Gets to column.
        /// </summary>
        /// <value>
        /// To column.
        /// </value>
        public string ToColumn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinClause" /> struct.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="joinType">Type of the join.</param>
        /// <param name="toTableName">Name of to table.</param>
        /// <param name="toColumnName">Name of to column.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="fromTableName">Name of from table.</param>
        /// <param name="fromColumnName">Name of from column.</param>
        public JoinClause(ISelectStringBuilder builder, JoinType joinType, string toTableName, string toColumnName,
            ComparisonOperator @operator,
            string fromTableName, string fromColumnName) : this(builder, joinType, toTableName)
        {
            FromTable = fromTableName;
            FromColumn = fromColumnName;
            ComparisonOperator = @operator;
            ToColumn = toColumnName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinClause"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="joinType">Type of the join.</param>
        /// <param name="toTableName">Name of to table.</param>
        public JoinClause(ISelectStringBuilder builder, JoinType joinType, string toTableName)
        {
            this.builder = builder;
            JoinType = joinType;
            ToTable = toTableName;
        }

        /// <summary>
        /// Sets the specified column name which table joins to.
        /// </summary>
        /// <param name="toColumnName">Name of to column.</param>
        /// <returns></returns>
        public JoinClause On(string toColumnName)
        {
            ToColumn = toColumnName;
            return this;
        }

        /// <summary>
        /// Finishes the clause initialization with <see cref="ComparisonOperator.Equals" /> comparison operator.
        /// </summary>
        /// <param name="fromTableName">Name of from table.</param>
        /// <param name="fromColumnName">Name of from column.</param>
        /// <returns></returns>
        public ISelectStringBuilder EqualsTo(string fromTableName, string fromColumnName)
        {
            ComparisonOperator = ComparisonOperator.Equals;
            FromTable = fromTableName;
            FromColumn = fromColumnName;
            return builder;
        }
    }
}
