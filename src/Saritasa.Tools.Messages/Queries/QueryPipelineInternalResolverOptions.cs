// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Queries
{
    /// <inheritdoc />
    public class QueryPipelineInternalResolverOptions : InternalResolverOptions
    {
        /// <summary>
        /// Assemblies to search handlers.
        /// </summary>
        public IList<Assembly> Assemblies { get; set; } = new List<Assembly>();

        /// <summary>
        /// Sets assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies to set.</param>
        public void SetAssemblies(params Assembly[] assemblies)
        {
            Assemblies = assemblies.ToList();
        }

        /// <summary>
        /// Whether assemblies are defined.
        /// </summary>
        internal bool HasAssemblies => Assemblies != null && Assemblies.Count > 0;
    }
}
