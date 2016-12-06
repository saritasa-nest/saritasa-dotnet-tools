using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Internal.Clauses
{
    /// <summary>
    /// Represents a ORDER BY clause to be used with SELECT statements
    /// </summary>
    public class OrderByClause
    {
        private readonly SelectStringBuilder builder;

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        public string ColumnName { get; }

        /// <summary>
        /// Gets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public SortingOperator SortOrder { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByClause"/> struct.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="column">The column.</param>
        public OrderByClause(SelectStringBuilder builder, string column) : this(builder, column, SortingOperator.Ascending)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByClause"/> struct.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="column">The column.</param>
        /// <param name="order">The order.</param>
        public OrderByClause(SelectStringBuilder builder, string column, SortingOperator order)
        {
            this.builder = builder;
            ColumnName = column;
            SortOrder = order;
        }
    }
}
