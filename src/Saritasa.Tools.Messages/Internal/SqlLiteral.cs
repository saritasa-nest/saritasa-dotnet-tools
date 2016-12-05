namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlLiteral
    {
        /// <summary>
        /// Gets or sets the statement rows affected.
        /// </summary>
        /// <value>
        /// The statement rows affected.
        /// </value>
        public static string StatementRowsAffected { get; set; } = "SELECT @@ROWCOUNT";

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlLiteral"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public SqlLiteral(string value)
        {
            Value = value;
        }
    }

}
