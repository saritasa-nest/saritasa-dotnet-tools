// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// The class is used to get messages to display for end user.
    /// </summary>
    public class DomainErrorDescriber
    {
        /// <summary>
        /// The default describer.
        /// </summary>
        public static DomainErrorDescriber Default { get; set; } = new DomainErrorDescriber();

        /// <summary>
        /// Get general error message.
        /// </summary>
        public virtual string Error() => Resources.Error;

        /// <summary>
        /// Get conflict error message.
        /// </summary>
        public virtual string Conflict() => Resources.Conflict;

        /// <summary>
        /// Get forbidden error message.
        /// </summary>
        public virtual string Forbidden() => Resources.Forbidden;

        /// <summary>
        /// Get not found error message.
        /// </summary>
        public virtual string NotFound() => Resources.NotFound;

        /// <summary>
        /// Get not found specified item error message.
        /// </summary>
        public virtual string NotFoundItem(string entityName) =>
            string.Format(Resources.NotFoundEntity, entityName);

        /// <summary>
        /// Get unauthorized error message.
        /// </summary>
        public virtual string Unauthorized() => Resources.Unauthorized;

        /// <summary>
        /// Get validation errors message.
        /// </summary>
        public virtual string ValidationErrors() => Resources.ValidationErrors;

        /// <summary>
        /// Get cannot find message.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        public virtual string CannotFind(string entityName) =>
            string.Format(Resources.CannotFind, entityName);
    }
}
