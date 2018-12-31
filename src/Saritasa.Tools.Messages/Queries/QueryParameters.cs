// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace Saritasa.Tools.Messages.Queries
{
    /// <summary>
    /// Query execution context parameters.
    /// </summary>
    internal class QueryParameters
    {
        /// <summary>
        /// Execution result.
        /// </summary>
        protected internal object Result { get; set; }

        /// <summary>
        /// Method to execute.
        /// </summary>
        protected internal MethodInfo Method { get; set; }

        /// <summary>
        /// Query object. The delegate will be executed against it.
        /// </summary>
        protected internal object QueryObject { get; set; }

        /// <summary>
        /// Function input parameters.
        /// </summary>
        protected internal object[] Parameters { get; set; }
    }
}
