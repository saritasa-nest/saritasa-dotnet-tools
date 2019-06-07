// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Saritasa.Tools.Domain;

namespace Saritasa.Tools.EF
{
    /// <summary>
    /// Entity Framework repository implementation that supports <see cref="IQueryable" />.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TContext">Database context type.</typeparam>
    public class EFQueryableRepository<TEntity, TContext> : EFRepository<TEntity, TContext>, IQueryableRepository<TEntity>
        where TEntity : class where TContext : DbContext
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">Database context.</param>
        public EFQueryableRepository(TContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public Type ElementType => typeof(TEntity);

        /// <inheritdoc />
        public Expression Expression => ((IQueryable<TEntity>)Set).Expression;

        /// <inheritdoc />
        public IQueryProvider Provider => ((IQueryable<TEntity>)Set).Provider;

        /// <inheritdoc />
        public IEnumerator<TEntity> GetEnumerator() => ((IQueryable<TEntity>)Set).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IQueryable<TEntity>)Set).GetEnumerator();
    }
}
