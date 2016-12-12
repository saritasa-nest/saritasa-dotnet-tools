// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// Wrap the string value as SQL source code literal (that is a string value will be recorded without quotes).
    /// </summary>
    internal class SqlLiteral
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlLiteral" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public SqlLiteral(string value)
        {
            Value = value;
        }
    }
}
