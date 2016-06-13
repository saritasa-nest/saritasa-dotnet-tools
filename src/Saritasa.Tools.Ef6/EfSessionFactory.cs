// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.EntityFramework
{
    using System.Data.Entity;
    using Domain;

    /// <summary>
    /// Entity framework session factory implementation.
    /// </summary>
    public class EfSessionFactory<TContext> : IUnitOfWorkFactory
        where TContext : DbContext, new()
    {
        /// <inheritdoc />
        public IUnitOfWork Create(System.Data.IsolationLevel isolationLevel)
        {
            return new EfSession<TContext>(new TContext());
        }

        /// <inheritdoc />
        public IUnitOfWork Create()
        {
            return new EfSession<TContext>(new TContext());
        }
    }
}
