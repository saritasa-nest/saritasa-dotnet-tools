using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Internal.Clauses
{
    /// <summary>
    /// Represents a ORDER BY clause to be used with SELECT statements
    /// </summary>
    public struct OrderByClause
    {
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
        /// <param name="column">The column.</param>
        public OrderByClause(string column)
        {
            ColumnName = column;
            SortOrder = SortingOperator.Ascending;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByClause"/> struct.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="order">The order.</param>
        public OrderByClause(string column, SortingOperator order)
        {
            ColumnName = column;
            SortOrder = order;
        }
    }
}
