// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.EF
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Custom extensions for IQueryable.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Apply includes to query.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="source">Source queryable.</param>
        /// <param name="includes">Properties to include.</param>
        /// <returns>Queryable with included properties.</returns>
        public static IQueryable<TEntity> Include<TEntity, TProperty>(
            this IQueryable<TEntity> source,
            IEnumerable<Expression<Func<TEntity, TProperty>>> includes)
            where TEntity : class
        {
            var query = source;
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query;
        }
    }
}
