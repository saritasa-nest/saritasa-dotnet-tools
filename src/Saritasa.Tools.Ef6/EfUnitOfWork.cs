// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Ef
{
    using System;
    using System.Data.Entity;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;

    /// <summary>
    /// Entity framework base implementation of Unit of Work.
    /// </summary>
    /// <typeparam name="TContext">Context type should be inherited of DbContext.</typeparam>
    public class EfUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        /// <summary>
        /// Database context.
        /// </summary>
        protected TContext Context { get; private set; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="context">Database context.</param>
        public EfUnitOfWork(TContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Context = context;
        }

        /// <summary>
        /// Dispose object.
        /// </summary>
        /// <param name="disposing">Dispone managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
        }

        /// <inheritdoc />
        public int Complete()
        {
            return Context.SaveChanges();
        }

        /// <inheritdoc />
        public Task<int> CompleteAsync()
        {
            return Context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public Task<int> CompleteAsync(CancellationToken cancellationToken)
        {
            return Context.SaveChangesAsync(cancellationToken);
        }
    }
}
