// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Base options with assemblies property.
    /// </summary>
    public class BaseAssembliesOptions
    {
        /// <summary>
        /// Assemblies to search handlers.
        /// </summary>
        public IList<Assembly> Assemblies { get; set; }

        /// <summary>
        /// Set assemblies as params.
        /// </summary>
        /// <param name="assemblies">Assemblies to set.</param>
        public void SetAssemblies(params Assembly[] assemblies)
        {
            Assemblies = assemblies.ToList();
        }

        /// <summary>
        /// Get assemblies as array. Returns empty array if source assemblies not defined.
        /// </summary>
        /// <returns>Assemblies list.</returns>
        internal Assembly[] GetAssemblies()
        {
            return Assemblies != null ? Assemblies.ToArray() : null;
        }

        /// <summary>
        /// Assemblies are defined.
        /// </summary>
        internal bool HasAssemblies => Assemblies != null && Assemblies.Count > 0;
    }
}
