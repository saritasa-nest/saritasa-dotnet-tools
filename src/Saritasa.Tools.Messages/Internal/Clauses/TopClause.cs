using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Internal.Clauses
{
    /// <summary>
    /// Represents a TOP clause for SELECT statements
    /// </summary>
    public struct TopClause
    {
        /// <summary>
        /// Gets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        public int Quantity { get; private set; }

        /// <summary>
        /// Gets the unit.
        /// </summary>
        /// <value>
        /// The unit.
        /// </value>
        public TopUnit Unit { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopClause"/> struct.
        /// </summary>
        /// <param name="nr">The nr.</param>
        public TopClause(int nr)
        {
            Quantity = nr;
            Unit = TopUnit.Records;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopClause"/> struct.
        /// </summary>
        /// <param name="nr">The nr.</param>
        /// <param name="aUnit">a unit.</param>
        public TopClause(int nr, TopUnit aUnit)
        {
            Quantity = nr;
            Unit = aUnit;
        }
    }
}