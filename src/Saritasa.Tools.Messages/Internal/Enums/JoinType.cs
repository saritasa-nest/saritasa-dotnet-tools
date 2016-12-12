// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Internal.Enums
{
    /// <summary>
    /// Represents operators for JOIN clauses.
    /// </summary>
    internal enum JoinType
    {
        /// <summary>
        /// The inner join.
        /// </summary>
        InnerJoin,

        /// <summary>
        /// The outer join.
        /// </summary>
        OuterJoin,

        /// <summary>
        /// The left join.
        /// </summary>
        LeftJoin,

        /// <summary>
        /// The right join.
        /// </summary>
        RightJoin
    }
}
