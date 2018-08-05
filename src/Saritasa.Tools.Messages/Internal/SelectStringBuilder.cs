// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Saritasa.Tools.Messages.Internal.Clauses;
using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// The SELECT statement abstract builder.
    /// </summary>
    /// <seealso cref="Saritasa.Tools.Messages.Internal.ISelectStringBuilder" />
    internal abstract class SelectStringBuilder : ISelectStringBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectStringBuilder"/> class.
        /// </summary>
        protected SelectStringBuilder()
        {
            SelectedTable = string.Empty;
            WhereStatement = new List<WhereClause>();
            OrderByStatement = new List<OrderByClause>();
        }

        /// <inheritdoc />
        public string SelectedTable { get; set; }

        /// <inheritdoc />
        public IList<WhereClause> WhereStatement { get; }

        /// <inheritdoc />
        public IList<OrderByClause> OrderByStatement { get; }

        /// <inheritdoc />
        public int? SkipRows { get; set; }

        /// <inheritdoc />
        public int? TakeRows { get; set; }

        /// <inheritdoc />
        public ISelectStringBuilder SelectAll()
        {
            return this;
        }

        /// <inheritdoc />
        public ISelectStringBuilder From(string tableName)
        {
            SelectedTable = tableName;
            return this;
        }

        /// <inheritdoc />
        public WhereClause Where(string columnName)
        {
            var clause = new WhereClause(this, columnName);
            WhereStatement.Add(clause);
            return clause;
        }

        /// <inheritdoc />
        public ISelectStringBuilder Where(string columnName, ComparisonOperator @operator, object value)
        {
            var clause = new WhereClause(this, columnName, @operator, value);
            WhereStatement.Add(clause);
            return this;
        }

        /// <inheritdoc />
        public ISelectStringBuilder OrderBy(string columnName)
        {
            var clause = new OrderByClause(columnName);
            OrderByStatement.Add(clause);
            return this;
        }

        /// <inheritdoc />
        public ISelectStringBuilder OrderBy(string columnName, SortingOperator @operator)
        {
            var clause = new OrderByClause(columnName, @operator);
            OrderByStatement.Add(clause);
            return this;
        }

        /// <inheritdoc />
        public ISelectStringBuilder Skip(int rows)
        {
            SkipRows = rows;
            return this;
        }

        /// <inheritdoc />
        public ISelectStringBuilder Take(int rows)
        {
            TakeRows = rows;
            return this;
        }

        /// <inheritdoc />
        public abstract string Build();
    }
}
