using System;
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
    /// <seealso cref="Saritasa.Tools.Messages.Internal.SelectStringBuilder" />
    public class SqlServerSelectStringBuilder : SelectStringBuilder
    {
        /// <inheritdoc />
        public override DbCommand BuildCommand()
        {
            return (DbCommand) this.BuildQuery(true);
        }

        /// <inheritdoc />
        public override string BuildQuery()
        {
            return (string) this.BuildQuery(false);
        }

        /// <summary>
        /// Builds the select query
        /// </summary>
        /// <returns>Returns a string containing the query, or a DbCommand containing a command with parameters</returns>
        private object BuildQuery(bool buildCommand)
        {
            if (buildCommand && dbProviderFactory == null)
                throw new Exception(
                    "Cannot build a command when the Db Factory hasn't been specified. Call SetDbProviderFactory first.");

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
                        case JoinType.InnerJoin:
                            sb.Append(" INNER JOIN");
                            break;
                        case JoinType.OuterJoin:
                            sb.Append(" OUTER JOIN");
                            break;
                        case JoinType.LeftJoin:
                            sb.Append(" LEFT JOIN");
                            break;
                        case JoinType.RightJoin:
                            sb.Append(" RIGHT JOIN");
                            break;
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
                sb.Append(string.Join(", ", orderByStatement.Select(clause =>
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

                if (offset.HasValue)
                {
                    sb.Append($" OFFSET {offset} ROWS");
                    if (fetch.HasValue)
                    {
                        sb.Append($" FETCH NEXT {fetch} ROWS ONLY");
                    }
                }
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
