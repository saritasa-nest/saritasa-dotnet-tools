// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.EFCore
{
    using System;
    using System.Data;
    using Microsoft.EntityFrameworkCore;
    using Domain;

    /// <summary>
    /// Unit of work factory implementation for Entity Framework 6. Note that it does not support
    /// to specify isolation level on creation.
    /// </summary>
    /// <typeparam name="TContext">Context type should be inherited of DbContext.</typeparam>
    public class EFUnitOfWorkFactory<TContext> : IUnitOfWorkFactory<EFUnitOfWork<TContext>>
        where TContext : DbContext, new()
    {
        /// <inheritdoc />
        public EFUnitOfWork<TContext> Create()
        {
            return new EFUnitOfWork<TContext>(new TContext());
        }

        /// <inheritdoc />
        public EFUnitOfWork<TContext> Create(IsolationLevel isolationLevel)
        {
            return new EFUnitOfWork<TContext>(new TContext());
        }
    }
}
