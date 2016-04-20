//
// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
//

namespace Saritasa.Tools.Interfaces
{
    using System;
    using System.Data;

    /// <summary>
    /// Unit of work factory abstraction.
    /// </summary>
    public interface IUnitOfWorkFactory
    {
        /// <summary>
        /// Creates unit of work with specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">Isolation level.</param>
        /// <returns>Unit of work.</returns>
        IUnitOfWork Create(IsolationLevel isolationLevel);

        /// <summary>
        /// Creates unit of work with default isolation level.
        /// </summary>
        /// <returns>Unit of work.</returns>
        IUnitOfWork Create();
    }
}
