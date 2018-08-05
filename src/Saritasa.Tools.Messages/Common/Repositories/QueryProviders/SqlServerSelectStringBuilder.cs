// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text;
using Saritasa.Tools.Messages.Internal;
using Saritasa.Tools.Messages.Internal.Clauses;
using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Common.Repositories.QueryProviders
{
    /// <summary>
    /// The SELECT statement SQL server builder.
    /// </summary>
    /// <seealso cref="Saritasa.Tools.Messages.Internal.SelectStringBuilder" />
    internal class SqlServerSelectStringBuilder : SelectStringBuilder
    {
        /// <inheritdoc />
        public override string Build()
        {
            var sb = new StringBuilder("SELECT ");

            if (!SkipRows.HasValue && TakeRows.HasValue)
            {
                sb.Append($"TOP {TakeRows} ");
            }

            // Output column names.
            sb.Append("*");

            // Output table names.
            if (!string.IsNullOrEmpty(SelectedTable))
            {
                sb.Append($" FROM {WrapVariable(SelectedTable)}");
            }

            // Output where statement.
            if (WhereStatement.Any())
            {
                sb.AppendLine();
                sb.Append($"WHERE {string.Join(" AND ", WhereStatement.Select(BuildWhereClauseString))}");
            }

            // Output OrderBy statement.
            if (OrderByStatement.Any())
            {
                sb.AppendLine();
                sb.Append($"ORDER BY {string.Join(", ", OrderByStatement.Select(BuildOrderByClauseString))}");

                // Works only in SQL Server 2012 and upper.
                // TODO use BETWEEN if it require
                if (SkipRows.HasValue)
                {
                    sb.AppendLine();
                    sb.Append($"OFFSET {SkipRows} ROWS");
                    if (TakeRows.HasValue)
                    {
                        sb.AppendLine();
                        sb.Append($"FETCH NEXT {TakeRows} ROWS ONLY");
                    }
                }
            }

            // Return the built query.
            return sb.ToString();
        }

        private static string BuildOrderByClauseString(OrderByClause clause)
        {
            return clause.SortOrder == SortingOperator.Descending
                ? $"{WrapVariable(clause.ColumnName)} DESC"
                : $"{WrapVariable(clause.ColumnName)}";
        }

        private static string BuildWhereClauseString(WhereClause clause)
        {
            var sb = new StringBuilder();

            sb.Append(CreateComparisonClause(clause.ColumnName, clause.Operator, clause.Value));

            return $"({sb})";
        }

        private static string CreateComparisonClause(string columnName, ComparisonOperator comparisonOperatorOperator, object value)
        {
            if (value == null || value == DBNull.Value)
            {
                switch (comparisonOperatorOperator)
                {
                    case ComparisonOperator.Equals:
                        return $"{WrapVariable(columnName)} IS NULL";
                    case ComparisonOperator.NotEquals:
                        return $"NOT {WrapVariable(columnName)} IS NULL";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(comparisonOperatorOperator),
                            string.Format(Properties.Strings.CannotUseComparisonOperatorNull, comparisonOperatorOperator));
                }
            }

            switch (comparisonOperatorOperator)
            {
                case ComparisonOperator.Equals:
                    return $"{WrapVariable(columnName)} = {FormatSqlValue(value)}";
                case ComparisonOperator.NotEquals:
                    return $"{WrapVariable(columnName)} <> {FormatSqlValue(value)}";
                case ComparisonOperator.GreaterThan:
                    return $"{WrapVariable(columnName)} > {FormatSqlValue(value)}";
                case ComparisonOperator.GreaterOrEquals:
                    return $"{WrapVariable(columnName)} >= {FormatSqlValue(value)}";
                case ComparisonOperator.LessThan:
                    return $"{WrapVariable(columnName)} < {FormatSqlValue(value)}";
                case ComparisonOperator.LessOrEquals:
                    return $"{WrapVariable(columnName)} <= {FormatSqlValue(value)}";
                case ComparisonOperator.Like:
                    return $"{WrapVariable(columnName)} LIKE {FormatSqlValue(value)}";
                case ComparisonOperator.NotLike:
                    return $"NOT {WrapVariable(columnName)} LIKE {FormatSqlValue(value)}";
                case ComparisonOperator.In:
                    return $"{WrapVariable(columnName)} IN ({FormatSqlValue(value)})";
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparisonOperatorOperator),
                        string.Format(Properties.Strings.CannotUseComparisonOperator, comparisonOperatorOperator));
            }
        }

        private static string FormatSqlValue(object someValue)
        {
            if (someValue == null || someValue is DBNull)
            {
                return "NULL";
            }
            if (someValue is Guid)
            {
                return $"\'{(Guid)someValue}\'";
            }
            if (someValue is string)
            {
                return $"\'{((string)someValue).Replace("'", "''")}\'";
            }
            if (someValue is DateTime)
            {
                return $"\'{(DateTime)someValue:yyyy-MM-dd hh:mm:ss}\'";
            }
            if (someValue is bool)
            {
                return (bool)someValue ? "1" : "0";
            }
            if (someValue is SqlLiteral)
            {
                return WrapVariable(((SqlLiteral)someValue).Value);
            }

            return someValue.ToString();
        }

        private static string WrapVariable(string arg)
        {
            return string.Join(".", arg.Split('.').Select(s => $"[{s}]"));
        }
    }
}
