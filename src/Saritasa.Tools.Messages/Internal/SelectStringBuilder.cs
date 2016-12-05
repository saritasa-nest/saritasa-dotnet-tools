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
    public class SelectStringBuilder
    {
        private DbProviderFactory dbProviderFactory;

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
        public SelectStringBuilder()
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
        public SelectStringBuilder(DbProviderFactory factory) : this()
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
            var newOrderByClause = new OrderByClause(field, order);
            orderByStatement.Add(newOrderByClause);
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
        public DbCommand BuildCommand()
        {
            return (DbCommand)this.BuildQuery(true);
        }

        /// <summary>
        /// Builds the query.
        /// </summary>
        /// <returns></returns>
        public string BuildQuery()
        {
            return (string)this.BuildQuery(false);
        }

        /// <summary>
        /// Builds the select query
        /// </summary>
        /// <returns>Returns a string containing the query, or a DbCommand containing a command with parameters</returns>
        private object BuildQuery(bool buildCommand)
        {
            if (buildCommand && dbProviderFactory == null)
                throw new Exception("Cannot build a command when the Db Factory hasn't been specified. Call SetDbProviderFactory first.");

            DbCommand command = null;
            if (buildCommand)
                command = dbProviderFactory.CreateCommand();

            var sb = new StringBuilder("SELECT ");

            // Output Distinct
            if (Distinct)
            {
                sb.Append("DISTINCT ");
            }

            // Output Top clause
            if (!(TopClause.Quantity == 100 && TopClause.Unit == TopUnit.Percent))
            {
                sb.Append($"TOP {TopClause.Quantity} ");
                if (TopClause.Unit == TopUnit.Percent)
                {
                    sb.Append("PERCENT ");
                }
            }

            // Output column names
            if (selectedColumns.Any())
            {
                sb.Append(string.Join(", ", selectedColumns));
            }
            else
            {
                if (SelectedTables.Count == 1)
                {
                    // By default only select * from the table that was selected. If there are any joins, it is the responsibility of the user to select the needed columns.
                    sb.Append(SelectedTables[0] + ".");
                }
                sb.Append("*");
            }

            // Output table names
            if (SelectedTables.Count > 0)
            {
                sb.Append($" FROM {string.Join(", ", SelectedTables)}");
            }

            // Output joins
            if (joins.Count > 0)
            {
                foreach (var clause in joins)
                {
                    switch (clause.JoinType)
                    {
                        case JoinType.InnerJoin: sb.Append(" INNER JOIN"); break;
                        case JoinType.OuterJoin: sb.Append(" OUTER JOIN"); break;
                        case JoinType.LeftJoin: sb.Append(" LEFT JOIN"); break;
                        case JoinType.RightJoin: sb.Append(" RIGHT JOIN"); break;
                    }
                    sb.Append($" {clause.ToTable} ON ");
                    sb.Append(WhereStatement.CreateComparisonClause(
                        $"{clause.FromTable}.{clause.FromColumn}",
                        clause.ComparisonOperator, 
                        new SqlLiteral($"{clause.ToTable}.{clause.ToColumn}")));
                    sb.Append(' ');
                }
            }

            // Output where statement
            if (WhereStatement.ClauseLevels > 0)
            {
                if (buildCommand)
                    sb.Append(" WHERE " + WhereStatement.BuildWhereStatement(() => command));
                else
                    sb.Append(" WHERE " + WhereStatement.BuildWhereStatement());
            }

            // Output GroupBy statement
            if (groupByColumns.Count > 0)
            {
                sb.Append($" GROUP BY {string.Join(", ", groupByColumns)} ");
            }

            // Output having statement
            if (Having.ClauseLevels > 0)
            {
                // Check if a Group By Clause was set
                if (groupByColumns.Count == 0)
                {
                    throw new Exception("Having statement was set without Group By");
                }
                if (buildCommand)
                    sb.Append(" HAVING " + Having.BuildWhereStatement(() => command));
                else
                    sb.Append(" HAVING " + Having.BuildWhereStatement());
            }

            // Output OrderBy statement
            if (orderByStatement.Count > 0)
            {
                sb.Append(" ORDER BY ");
                sb.Append(string.Join(",", orderByStatement.Select(clause =>
                {
                    switch (clause.SortOrder)
                    {
                        case SortingOperator.Ascending:
                            return $"{clause.ColumnName} ASC";
                        case SortingOperator.Descending:
                            return $"{clause.ColumnName} DESC";
                        default:
                            return "";
                    }
                })));
                sb.Append(' ');
            }

            if (buildCommand)
            {
                // Return the build command
                command.CommandText = sb.ToString();
                return command;
            }

            // Return the built query
            return sb.ToString();
        }
    }

}
