// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Ef
{
    using System;
    using System.Data;
    using System.Data.Entity;
    using Domain;

    /// <summary>
    /// Unit of work factory implementation for Entity Framework 6. Note that it does not support
    /// to specify isolation level on creation.
    /// </summary>
    /// <typeparam name="TContext">Context type should be inherited of DbContext.</typeparam>
    public class EfUnitOfWorkFactory<TContext> : IUnitOfWorkFactory<EfUnitOfWork<TContext>>
        where TContext : DbContext, new()
    {
        /// <inheritdoc />
        public EfUnitOfWork<TContext> Create()
        {
            return new EfUnitOfWork<TContext>(new TContext());
        }

        /// <inheritdoc />
        public EfUnitOfWork<TContext> Create(IsolationLevel isolationLevel)
        {
            return new EfUnitOfWork<TContext>(new TContext());
        }
    }
}
