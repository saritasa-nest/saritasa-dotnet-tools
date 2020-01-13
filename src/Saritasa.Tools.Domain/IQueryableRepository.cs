// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// Repository that also implements IQueryable interface.
    /// </summary>
    /// <typeparam name="TEntity">The entity type that repository wraps.</typeparam>
    [Obsolete("Prefer using DbSet or custom repository instead of generic repository.")]
    public interface IQueryableRepository<TEntity> : IRepository<TEntity>, IQueryable<TEntity>
        where TEntity : class
    {
    }
}
