// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain
{
    using System.Linq;

    /// <summary>
    /// Repository that also implements IQueryable interface.
    /// </summary>
    /// <typeparam name="TEntity">The entity repository wraps.</typeparam>
    public interface IQueryableRepository<TEntity> : IRepository<TEntity>, IQueryable<TEntity>
        where TEntity : class
    {
    }
}
