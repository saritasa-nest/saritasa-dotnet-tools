using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Internal.Clauses
{
    /// <summary>
    /// 
    /// </summary>
    public class WhereStatement : List<List<WhereClause>>
    {
        // The list in this container will contain lists of clauses, and 
        // forms a where statement alltogether!

        /// <summary>
        /// Gets the clause levels.
        /// </summary>
        /// <value>
        /// The clause levels.
        /// </value>
        public int ClauseLevels => this.Count;

        private void AssertLevelExistance(int level)
        {
            if (this.Count < (level - 1))
            {
                throw new Exception($"Level {level} not allowed because level {level - 1} does not exist.");
            }
            // Check if new level must be created
            else if (this.Count < level)
            {
                this.Add(new List<WhereClause>());
            }
        }

        /// <summary>
        /// Adds the specified clause.
        /// </summary>
        /// <param name="clause">The clause.</param>
        public void Add(WhereClause clause)
        {
            this.Add(clause, 1);
        }

        /// <summary>
        /// Adds the specified clause.
        /// </summary>
        /// <param name="clause">The clause.</param>
        /// <param name="level">The level.</param>
        public void Add(WhereClause clause, int level)
        {
            this.AddWhereClauseToLevel(clause, level);
        }

        /// <summary>
        /// Adds the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <returns></returns>
        public WhereClause Add(string field, ComparisonOperator @operator, object compareValue)
        {
            return this.Add(field, @operator, compareValue, 1);
        }

        /// <summary>
        /// Adds the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <returns></returns>
        public WhereClause Add(Enum field, ComparisonOperator @operator, object compareValue)
        {
            return this.Add(field.ToString(), @operator, compareValue, 1);
        }

        /// <summary>
        /// Adds the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public WhereClause Add(string field, ComparisonOperator @operator, object compareValue, int level)
        {
            var newWhereClause = new WhereClause(null, field, @operator, compareValue);
            this.AddWhereClauseToLevel(newWhereClause, level);
            return newWhereClause;
        }

        private void AddWhereClause(WhereClause clause)
        {
            AddWhereClauseToLevel(clause, 1);
        }

        private void AddWhereClauseToLevel(WhereClause clause, int level)
        {
            // Add the new clause to the array at the right level
            AssertLevelExistance(level);
            this[level - 1].Add(clause);
        }

        /// <summary>
        /// Builds the where statement.
        /// </summary>
        /// <param name="usedDbCommand">The used database command.</param>
        /// <returns></returns>
        public string BuildWhereStatement(Func<DbCommand> usedDbCommand = null)
        {
            return string.Join(" OR ", this.Select(s => $"({string.Join(" AND ", s.Select(c => BuildWhereClauseString(c, usedDbCommand)))})"));
        }

        private static string BuildWhereClauseString(WhereClause clause, Func<DbCommand> usedDbCommand)
        {
            var whereClause = new StringBuilder();

            if (usedDbCommand != null)
            {
                // Create a parameter
                var parameterName = $"@p{usedDbCommand().Parameters.Count + 1}_{clause.ColumnName.Replace('.', '_')}";

                var parameter = usedDbCommand().CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.Value = clause.Value;
                usedDbCommand().Parameters.Add(parameter);

                // Create a where clause using the parameter, instead of its value
                whereClause.Append(CreateComparisonClause(clause.ColumnName, clause.Operator, new SqlLiteral(parameterName)));
            }
            else
            {
                whereClause.Append(CreateComparisonClause(clause.ColumnName, clause.Operator, clause.Value));
            }

            foreach (var subWhereClause in clause.SubClauses)   // Loop through all subclauses, append them together with the specified logic operator
            {
                switch (subWhereClause.LogicOperator)
                {
                    case LogicOperator.And:
                        whereClause.Append(" AND "); break;
                    case LogicOperator.Or:
                        whereClause.Append(" OR "); break;
                }

                if (usedDbCommand != null)
                {
                    // Create a parameter
                    var parameterName = $"@p{usedDbCommand().Parameters.Count + 1}_{clause.ColumnName.Replace('.', '_')}";

                    var parameter = usedDbCommand().CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Value = subWhereClause.Value;
                    usedDbCommand().Parameters.Add(parameter);

                    // Create a where clause using the parameter, instead of its value
                    whereClause.Append(CreateComparisonClause(clause.ColumnName, subWhereClause.ComparisonOperator, new SqlLiteral(parameterName)));
                }
                else
                {
                    whereClause.Append(CreateComparisonClause(clause.ColumnName, subWhereClause.ComparisonOperator, subWhereClause.Value));
                }
            }

            return $"({whereClause})";
        }

        internal static string CreateComparisonClause(string fieldName, ComparisonOperator comparisonOperatorOperator, object value)
        {
            if (value == null || value == DBNull.Value)
            {
                switch (comparisonOperatorOperator)
                {
                    case ComparisonOperator.Equals:
                        return $"{fieldName} IS NULL";
                    case ComparisonOperator.NotEquals:
                        return $"NOT {fieldName} IS NULL";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(comparisonOperatorOperator),
                            $"Cannot use comparison operator {comparisonOperatorOperator} for NULL values.");
                }
            }

            switch (comparisonOperatorOperator)
            {
                case ComparisonOperator.Equals:
                    return $"{fieldName} = {FormatSqlValue(value)}";
                case ComparisonOperator.NotEquals:
                    return $"{fieldName} <> {FormatSqlValue(value)}";
                case ComparisonOperator.GreaterThan:
                    return $"{fieldName} > {FormatSqlValue(value)}";
                case ComparisonOperator.GreaterOrEquals:
                    return $"{fieldName} >= {FormatSqlValue(value)}";
                case ComparisonOperator.LessThan:
                    return $"{fieldName} < {FormatSqlValue(value)}";
                case ComparisonOperator.LessOrEquals:
                    return $"{fieldName} <= {FormatSqlValue(value)}";
                case ComparisonOperator.Like:
                    return $"{fieldName} LIKE {FormatSqlValue(value)}";
                case ComparisonOperator.NotLike:
                    return $"NOT {fieldName} LIKE {FormatSqlValue(value)}";
                case ComparisonOperator.In:
                    return $"{fieldName} IN ({FormatSqlValue(value)})";
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparisonOperatorOperator), 
                        $"Cannot use comparison operator {comparisonOperatorOperator}.");
            }
        }

        /// <summary>
        /// Formats the SQL value.
        /// </summary>
        /// <param name="someValue">Some value.</param>
        /// <returns></returns>
        internal static string FormatSqlValue(object someValue)
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
                return $"\'{(DateTime)someValue:yyyy/MM/dd hh:mm:ss}\'";
            }
            if (someValue is bool)
            {
                return (bool)someValue ? "1" : "0";
            }
            if (someValue is SqlLiteral)
            {
                return ((SqlLiteral)someValue).Value;
            }

            return someValue.ToString();
        }

        /// <summary>
        /// This static method combines 2 where statements with eachother to form a new statement
        /// </summary>
        /// <param name="statement1"></param>
        /// <param name="statement2"></param>
        /// <returns></returns>
        public static WhereStatement CombineStatements(WhereStatement statement1, WhereStatement statement2)
        {
            // statement1: {Level1}((Age<15 OR Age>=20) AND (strEmail LIKE 'e%') OR {Level2}(Age BETWEEN 15 AND 20))
            // Statement2: {Level1}((Name = 'Peter'))
            // Return statement: {Level1}((Age<15 or Age>=20) AND (strEmail like 'e%') AND (Name = 'Peter'))

            // Make a copy of statement1
            var result = WhereStatement.Copy(statement1);

            // Add all clauses of statement2 to result
            for (var i = 0; i < statement2.ClauseLevels; i++) // for each clause level in statement2
            {
                var level = statement2[i];
                foreach (var clause in level) // for each clause in level i
                {
                    for (var j = 0; j < result.ClauseLevels; j++)  // for each level in result, add the clause
                    {
                        result.AddWhereClauseToLevel(clause, j);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Copies the specified statement.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <returns></returns>
        public static WhereStatement Copy(WhereStatement statement)
        {
            var result = new WhereStatement();
            var currentLevel = 0;
            foreach (var level in statement)
            {
                currentLevel++;
                result.Add(new List<WhereClause>());
                foreach (var clause in statement[currentLevel - 1])
                {
                    var clauseCopy = new WhereClause(null, clause.ColumnName, clause.Operator, clause.Value);
                    foreach (var subClause in clause.SubClauses)
                    {
                        var subClauseCopy = new WhereClause.SubClause(subClause.LogicOperator, subClause.ComparisonOperator, subClause.Value);
                        clauseCopy.SubClauses.Add(subClauseCopy);
                    }
                    result[currentLevel - 1].Add(clauseCopy);
                }
            }
            return result;
        }
    }
}