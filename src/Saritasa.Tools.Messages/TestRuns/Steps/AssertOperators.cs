// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.TestRuns.Steps
{
    /// <summary>
    /// Assert possible comparisions.
    /// </summary>
    public enum AssertOperators
    {
        /// <summary>
        /// Equal to value.
        /// </summary>
        Equal,

        /// <summary>
        /// Not equal to value.
        /// </summary>
        NotEqual,

        /// <summary>
        /// Greater to value.
        /// </summary>
        Greater,

        /// <summary>
        /// Greater or equal to value.
        /// </summary>
        GreaterOrEqual,

        /// <summary>
        /// Less than value.
        /// </summary>
        Less,

        /// <summary>
        /// Less or equal to value.
        /// </summary>
        LessOrEqual,

        /// <summary>
        /// Value is <c>true</c>.
        /// </summary>
        IsTrue,

        /// <summary>
        /// Value is <c>false</c>.
        /// </summary>
        IsFalse,

        /// <summary>
        /// Value is zero.
        /// </summary>
        IsZero,

        /// <summary>
        /// Value is above zero.
        /// </summary>
        IsAboveZero,

        /// <summary>
        /// Value is below zero.
        /// </summary>
        IsBelowZero
    }
}
