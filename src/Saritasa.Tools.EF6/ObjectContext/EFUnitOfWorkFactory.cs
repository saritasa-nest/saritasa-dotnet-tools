// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Domain;

namespace Saritasa.Tools.EF.ObjectContext
{
    /// <summary>
    /// Unit of work factory implementation for Entity Framework 6. Note that it does not support
    /// to specify isolation level on creation. Based on <see cref="System.Data.Entity.Core.Objects.ObjectContext" />.
    /// </summary>
    /// <typeparam name="TContext">Context type should be inherited of DbContext.</typeparam>
    public class EFUnitOfWorkFactory<TContext> : IUnitOfWorkFactory<EFUnitOfWork<TContext>>
        where TContext : System.Data.Entity.Core.Objects.ObjectContext, new()
    {
        private readonly Func<TContext> createContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EFUnitOfWorkFactory()
        {
            createContext = DynamicModuleLambdaCompiler.GenerateFactory<TContext>();
        }

        /// <inheritdoc />
        public virtual EFUnitOfWork<TContext> Create()
        {
            return new EFUnitOfWork<TContext>(createContext());
        }
    }
}
