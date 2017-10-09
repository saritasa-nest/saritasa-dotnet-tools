// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Data;

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// The class provides existing unit of work instead of creating new one. It may be useful for example
    /// when you already have Unit of work instance and you want to call another method that requires
    /// <see cref="Domain.IUnitOfWorkFactory{T}" /> interface. You should instaniate the class and pass your unit of work.
    /// </summary>
    /// <typeparam name="TUnitOfWork">Unit of work type.</typeparam>
    public class InstanceUnitOfWorkFactory<TUnitOfWork> : IUnitOfWorkFactory<TUnitOfWork> where TUnitOfWork : class
    {
        private readonly TUnitOfWork unitOfWork;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="unitOfWork">Application unit of work.</param>
        public InstanceUnitOfWorkFactory(TUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }
            this.unitOfWork = unitOfWork;
        }

        /// <inheritdoc />
        public TUnitOfWork Create()
        {
            return unitOfWork;
        }

        /// <inheritdoc />
        public TUnitOfWork Create(IsolationLevel isolationLevel)
        {
            return unitOfWork;
        }
    }
}
