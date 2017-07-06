// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Saritasa.Tools.Domain;

namespace Saritasa.Tools.EFCore
{
    /// <summary>
    /// Entity framework base implementation of Unit of Work.
    /// </summary>
    /// <typeparam name="TContext">Context type should be inherited of DbContext.</typeparam>
    public class EFUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        /// <summary>
        /// Database context.
        /// </summary>
        protected virtual TContext Context { get; private set; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="context">Database context.</param>
        public EFUnitOfWork(TContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Context = context;
        }

        private bool disposed;

        /// <summary>
        /// Dispose object.
        /// </summary>
        /// <param name="disposing">Dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Context != null)
                    {
                        Context.Dispose();
                        Context = null;
                    }
                }
                disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public virtual int SaveChanges()
        {
            return Context.SaveChanges();
        }

        /// <inheritdoc />
        public virtual Task<int> SaveChangesAsync()
        {
            return Context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Context.SaveChangesAsync(cancellationToken);
        }
    }
}
