using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Internal.Clauses
{
    /// <summary>
    /// Represents a WHERE clause to be used with SELECT statements
    /// </summary>
    internal class WhereClause
    {
        private readonly ISelectStringBuilder builder;

        /// <summary>
        /// Gets/sets the name of the database column this WHERE clause should operate on
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Gets/sets the comparison method
        /// </summary>
        public ComparisonOperator Operator { get; private set; }

        /// <summary>
        /// Gets/sets the value that was set for comparison
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhereClause"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="column">The field.</param>
        /// <param name="compareOperator">The compare operator.</param>
        /// <param name="compareValue">The compare value.</param>
        public WhereClause(ISelectStringBuilder builder, string column, ComparisonOperator compareOperator, object compareValue)
        {
            this.builder = builder;
            ColumnName = column;
            Operator = compareOperator;
            Value = compareValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhereClause"/> struct.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="column">The column.</param>
        public WhereClause(ISelectStringBuilder builder, string column) : this(builder, column, ComparisonOperator.NotSet, null)
        {
        }

        /// <summary>
        /// Finishes the clause initialization with <see cref="ComparisonOperator.Equals"/> comparison operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ISelectStringBuilder EqualsTo(object value)
        {
            Operator = ComparisonOperator.Equals;
            Value = value;
            return builder;
        }

        /// <summary>
        /// Finishes the clause initialization with <see cref="ComparisonOperator.NotEquals"/> comparison operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ISelectStringBuilder NotEqualsTo(object value)
        {
            Operator = ComparisonOperator.NotEquals;
            Value = value;
            return builder;
        }

        /// <summary>
        /// Finishes the clause initialization with <see cref="ComparisonOperator.Like"/> comparison operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ISelectStringBuilder Like(object value)
        {
            Operator = ComparisonOperator.Like;
            Value = value;
            return builder;
        }

        /// <summary>
        /// Finishes the clause initialization with <see cref="ComparisonOperator.NotLike"/> comparison operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ISelectStringBuilder NotLike(object value)
        {
            Operator = ComparisonOperator.NotLike;
            Value = value;
            return builder;
        }

        /// <summary>
        /// Finishes the clause initialization with <see cref="ComparisonOperator.GreaterThan"/> comparison operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ISelectStringBuilder GreaterThan(object value)
        {
            Operator = ComparisonOperator.GreaterThan;
            Value = value;
            return builder;
        }

        /// <summary>
        /// Finishes the clause initialization with <see cref="ComparisonOperator.GreaterOrEquals"/> comparison operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ISelectStringBuilder GreaterOrEqualsTo(object value)
        {
            Operator = ComparisonOperator.GreaterOrEquals;
            Value = value;
            return builder;
        }

        /// <summary>
        /// Finishes the clause initialization with <see cref="ComparisonOperator.LessThan"/> comparison operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ISelectStringBuilder LessThan(object value)
        {
            Operator = ComparisonOperator.LessThan;
            Value = value;
            return builder;
        }

        /// <summary>
        /// Finishes the clause initialization with <see cref="ComparisonOperator.LessOrEquals"/> comparison operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ISelectStringBuilder LessOrEqualsTo(object value)
        {
            Operator = ComparisonOperator.LessOrEquals;
            Value = value;
            return builder;
        }

        /// <summary>
        /// Finishes the clause initialization with <see cref="ComparisonOperator.In"/> comparison operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ISelectStringBuilder In(object value)
        {
            Operator = ComparisonOperator.In;
            Value = value;
            return builder;
        }
    }
}