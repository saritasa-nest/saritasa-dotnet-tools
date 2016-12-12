namespace Saritasa.Tools.Messages.Internal.Enums
{
    /// <summary>
    /// Represents comparison operators for WHERE, HAVING and JOIN clauses
    /// </summary>
    internal enum ComparisonOperator
    {
        /// <summary>
        /// The comparison operator Equals
        /// </summary>
        Equals,
        /// <summary>
        /// The comparison operator Not Equals
        /// </summary>
        NotEquals,
        /// <summary>
        /// The comparison operator Like
        /// </summary>
        Like,
        /// <summary>
        /// The comparison operator Not Like
        /// </summary>
        NotLike,
        /// <summary>
        /// The comparison operator >
        /// </summary>
        GreaterThan,
        /// <summary>
        /// The comparison operator >=
        /// </summary>
        GreaterOrEquals,
        /// <summary>
        /// The comparison operator {
        /// </summary>
        LessThan,
        /// <summary>
        /// The comparison operator {=
        /// </summary>
        LessOrEquals,
        /// <summary>
        /// The comparison operator In
        /// </summary>
        In,
        /// <summary>
        /// The not set
        /// </summary>
        NotSet
    }
}
