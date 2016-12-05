using System;
using System.Collections.Generic;
using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Internal.Clauses
{
    /// <summary>
    /// Represents a WHERE clause on 1 database column, containing 1 or more comparisons on 
    /// that column, chained together by logic operators: eg (UserID=1 or UserID=2 or UserID>100)
    /// This can be achieved by doing this:
    /// WhereClause myWhereClause = new WhereClause("UserID", Comparison.Equals, 1);
    /// myWhereClause.AddClause(LogicOperator.Or, Comparison.Equals, 2);
    /// myWhereClause.AddClause(LogicOperator.Or, Comparison.GreaterThan, 100);
    /// </summary>
    public class WhereClause
    {
        private readonly SelectStringBuilder builder;

        internal class SubClause
        {
            public LogicOperator LogicOperator;
            public ComparisonOperator ComparisonOperator;
            public object Value;
            public SubClause(LogicOperator logic, ComparisonOperator compareOperator, object compareValue)
            {
                LogicOperator = logic;
                ComparisonOperator = compareOperator;
                Value = compareValue;
            }
        }
        internal List<SubClause> SubClauses;	// Array of SubClause

        /// <summary>
        /// Gets/sets the name of the database column this WHERE clause should operate on
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets/sets the comparison method
        /// </summary>
        public ComparisonOperator Operator { get; set; }

        /// <summary>
        /// Gets/sets the value that was set for comparison
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhereClause"/> struct.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="column">The field.</param>
        /// <param name="compareOperator">The first compare operator.</param>
        /// <param name="compareValue">The first compare value.</param>
        public WhereClause(SelectStringBuilder builder, string column, ComparisonOperator compareOperator, object compareValue)
        {
            this.builder = builder;
            ColumnName = column;
            Operator = compareOperator;
            Value = compareValue;
            SubClauses = new List<SubClause>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhereClause"/> struct.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="column">The column.</param>
        public WhereClause(SelectStringBuilder builder, string column) : this(builder, column, ComparisonOperator.NotSet, null)
        {
        }

        /// <summary>
        /// Adds an extra clause.
        /// </summary>
        /// <param name="logic">The logic.</param>
        /// <param name="compareOperator">The compare operator.</param>
        /// <param name="compareValue">The compare value.</param>
        public void AddClause(LogicOperator logic, ComparisonOperator compareOperator, object compareValue)
        {
            var newSubClause = new SubClause(logic, compareOperator, compareValue);
            SubClauses.Add(newSubClause);
        }

        /// <summary>
        /// Equalses to.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public SelectStringBuilder EqualsTo(object value)
        {
            Operator = ComparisonOperator.Equals;
            Value = value;
            return builder;
        }
    }
}