// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using Saritasa.Tools.Messages.Abstractions.Queries;

namespace Saritasa.Tools.Messages.Queries
{
    /// <summary>
    /// Query execution context.
    /// </summary>
    public class QueryParameters
    {
        /// <summary>
        /// Query handler.
        /// </summary>
        protected internal Type HandlerType { get; set; }

        /// <summary>
        /// Query handler method to execute.
        /// </summary>
        protected internal MethodInfo HandlerMethod { get; set; }

        /// <summary>
        /// Execution result.
        /// </summary>
        protected internal object Result { get; set; }

        /// <summary>
        /// Method to execute.
        /// </summary>
        protected internal MethodInfo Method { get; set; }

        /// <summary>
        /// Query object the delegate will be executed against.
        /// </summary>
        protected internal object QueryObject { get; set; }

        /// <summary>
        /// If <c>true</c> <see cref="QueryParameters" /> has been created by <see cref="IQueryCaller" />.
        /// </summary>
        protected internal bool FakeQueryObject { get; set; }

        /// <summary>
        /// Function input parameters.
        /// </summary>
        protected internal object[] Parameters { get; set; }
    }
}
