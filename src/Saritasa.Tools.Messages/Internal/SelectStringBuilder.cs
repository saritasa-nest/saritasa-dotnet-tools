using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Saritasa.Tools.Messages.Internal.Clauses;
using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SelectStringBuilder
    {
        protected DbProviderFactory dbProviderFactory;

        protected IList<string> selectedColumns;	// array of string
        protected IList<JoinClause> joins;	// array of JoinClause
        protected IList<OrderByClause> orderByStatement;	// array of OrderByClause
        protected IList<string> groupByColumns;		// array of string

        /// <summary>
        /// Gets or sets the where statement.
        /// </summary>
        /// <value>
        /// The where statement.
        /// </value>
        protected internal WhereStatement WhereStatement { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectStringBuilder"/> class.
        /// </summary>
        protected SelectStringBuilder()
        {
            Distinct = false;
            TopClause = new TopClause(100, TopUnit.Percent);
            selectedColumns = new List<string>();
            SelectedTables = new List<string>();
            WhereStatement = new WhereStatement();
            joins = new List<JoinClause>();
            orderByStatement = new List<OrderByClause>();
            groupByColumns = new List<string>();
            Having = new WhereStatement();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectStringBuilder"/> class.
        /// </summary>
        /// <param name="factory">The database provider factory.</param>
        protected SelectStringBuilder(DbProviderFactory factory) : this()
        {
            SetDbProviderFactory(factory);
        }
        
        /// <summary>
        /// Sets the database provider factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public void SetDbProviderFactory(DbProviderFactory factory)
        {
            dbProviderFactory = factory;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SelectStringBuilder"/> is distinct.
        /// </summary>
        /// <value>
        ///   <c>true</c> if distinct; otherwise, <c>false</c>.
        /// </value>
        public bool Distinct { get; set; }

        /// <summary>
        /// Gets or sets the top clause.
        /// </summary>
        /// <value>
        /// The top clause.
        /// </value>
        public TopClause TopClause { get; set; }

        /// <summary>
        /// Gets or sets the top records.
        /// </summary>
        /// <value>
        /// The top records.
        /// </value>
        public int TopRecords
        {
            get { return TopClause.Quantity; }
            set { TopClause = new TopClause(value, TopUnit.Records); }
        }

        /// <summary>
        /// Gets the selected columns.
        /// </summary>
        /// <value>
        /// The selected columns.
        /// </value>
        public IList<string> SelectedColumns => selectedColumns.Count > 0 
            ? selectedColumns 
            : new List<string> { "*" };

        /// <summary>
        /// Gets the selected tables.
        /// </summary>
        /// <value>
        /// The selected tables.
        /// </value>
        public IList<string> SelectedTables { get; }

        protected int? offset { get; set; }

        protected int? fetch { get; set; }

        /// <summary>
        /// Selects all columns.
        /// </summary>
        /// <returns></returns>
        public SelectStringBuilder SelectAll()
        {
            selectedColumns.Clear();
            return this;
        }
        /// <summary>
        /// Selects the count.
        /// </summary>
        public SelectStringBuilder SelectCount()
        {
            return Select("count(1)");
        }

        /// <summary>
        /// Selects the columns.
        /// </summary>
        /// <param name="columns">The columns.</param>
        public SelectStringBuilder Select(params string[] columns)
        {
            selectedColumns.Clear();
            foreach (var column in columns)
            {
                selectedColumns.Add(column);
            }
            return this;
        }

        /// <summary>
        /// Selects from tables.
        /// </summary>
        /// <param name="tables">The tables.</param>
        public SelectStringBuilder From(params string[] tables)
        {
            SelectedTables.Clear();
            foreach (var table in tables)
            {
                SelectedTables.Add(table);
            }
            return this;
        }

        /// <summary>
        /// Adds the join.
        /// </summary>
        /// <param name="newJoin">The new join.</param>
        public SelectStringBuilder AddJoin(JoinClause newJoin)
        {
            joins.Add(newJoin);
            return this;
        }
        /// <summary>
        /// Adds the join.
        /// </summary>
        /// <param name="join">The join.</param>
        /// <param name="toTableName">Name of to table.</param>
        /// <param name="toColumnName">Name of to column.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="fromTableName">Name of from table.</param>
        /// <param name="fromColumnName">Name of from column.</param>
        public SelectStringBuilder AddJoin(JoinType join, string toTableName, string toColumnName, ComparisonOperator @operator, string fromTableName, string fromColumnName)
        {
            var newJoin = new JoinClause(join, toTableName, toColumnName, @operator, fromTableName, fromColumnName);
            joins.Add(newJoin);
            return this;
        }

        /// <summary>
        /// Adds the where.
        /// </summary>
        /// <param name="clause">The clause.</param>
        public void Where(WhereClause clause)
        {
            Where(clause, 1);
        }

        /// <summary>
        /// Adds the where.
        /// </summary>
        /// <param name="clause">The clause.</param>
        /// <param name="level">The level.</param>
        public void Where(WhereClause clause, int level)
        {
            WhereStatement.Add(clause, level);
        }

        /// <summary>
        /// Adds the where.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <returns></returns>
        public WhereClause Where(string field, ComparisonOperator @operator, object compareValue)
        {
            return Where(field, @operator, compareValue, 1);
        }

        /// <summary>
        /// Adds the where.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <returns></returns>
        public WhereClause Where(Enum field, ComparisonOperator @operator, object compareValue)
        {
            return Where(field.ToString(), @operator, compareValue, 1);
        }

        /// <summary>
        /// Adds the where.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public WhereClause Where(string field, ComparisonOperator @operator, object compareValue, int level)
        {
            var newWhereClause = new WhereClause(this, field, @operator, compareValue);
            WhereStatement.Add(newWhereClause, level);
            return newWhereClause;
        }

        /// <summary>
        /// Adds the partial set where clause.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public WhereClause Where(string column)
        {
            var clause = new WhereClause(this, column);
            WhereStatement.Add(clause);
            return clause;
        }

        /// <summary>
        /// Adds the order by.
        /// </summary>
        /// <param name="clause">The clause.</param>
        public SelectStringBuilder OrderBy(OrderByClause clause)
        {
            orderByStatement.Add(clause);
            return this;
        }

        /// <summary>
        /// Adds the order by.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="order">The order.</param>
        public SelectStringBuilder OrderBy(Enum field, SortingOperator order)
        {
            return OrderBy(field.ToString(), order);
        }

        /// <summary>
        /// Adds the order by.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="order">The order.</param>
        public SelectStringBuilder OrderBy(string field, SortingOperator order)
        {
            var clause = new OrderByClause(this, field, order);
            orderByStatement.Add(clause);
            return this;
        }

        /// <summary>
        /// Adds the order by.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public SelectStringBuilder OrderBy(string field)
        {
            var clause = new OrderByClause(this, field);
            orderByStatement.Add(clause);
            return this;
        }

        /// <summary>
        /// Offsets the specified rows.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">orderByStatement - First you should specify the OrderBy statement.</exception>
        public SelectStringBuilder Offset(int rows)
        {
            if (!orderByStatement.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(orderByStatement), "You should specify the OrderBy statement.");
            }

            offset = rows;
            return this;
        }

        /// <summary>
        /// Fetches the specified rows.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// orderByStatement - You should specify the OrderBy statement.
        /// or
        /// offset - You should specify the offset value.
        /// </exception>
        public SelectStringBuilder Fetch(int rows)
        {
            if (!orderByStatement.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(orderByStatement), "You should specify the OrderBy statement.");
            }

            if (!offset.HasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "You should specify the offset value.");
            }

            fetch = rows;
            return this;
        }

        /// <summary>
        /// Groups the by.
        /// </summary>
        /// <param name="columns">The columns.</param>
        public SelectStringBuilder GroupBy(params string[] columns)
        {
            foreach (var column in columns)
            {
                groupByColumns.Add(column);
            }
            return this;
        }

        /// <summary>
        /// Gets the having.
        /// </summary>
        /// <value>
        /// The having.
        /// </value>
        public WhereStatement Having { get; }

        /// <summary>
        /// Adds the having.
        /// </summary>
        /// <param name="clause">The clause.</param>
        public void AddHaving(WhereClause clause)
        {
            AddHaving(clause, 1);
        }

        /// <summary>
        /// Adds the having.
        /// </summary>
        /// <param name="clause">The clause.</param>
        /// <param name="level">The level.</param>
        public void AddHaving(WhereClause clause, int level)
        {
            Having.Add(clause, level);
        }

        /// <summary>
        /// Adds the having.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <returns></returns>
        public WhereClause AddHaving(string field, ComparisonOperator @operator, object compareValue)
        {
            return AddHaving(field, @operator, compareValue, 1);
        }

        /// <summary>
        /// Adds the having.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <returns></returns>
        public WhereClause AddHaving(Enum field, ComparisonOperator @operator, object compareValue)
        {
            return AddHaving(field.ToString(), @operator, compareValue, 1);
        }

        /// <summary>
        /// Adds the having.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public WhereClause AddHaving(string field, ComparisonOperator @operator, object compareValue, int level)
        {
            var newWhereClause = new WhereClause(this, field, @operator, compareValue);
            Having.Add(newWhereClause, level);
            return newWhereClause;
        }

        /// <summary>
        /// Builds the command.
        /// </summary>
        /// <returns></returns>
        public abstract DbCommand BuildCommand();

        /// <summary>
        /// Builds the query.
        /// </summary>
        /// <returns></returns>
        public abstract string BuildQuery();
    }
}