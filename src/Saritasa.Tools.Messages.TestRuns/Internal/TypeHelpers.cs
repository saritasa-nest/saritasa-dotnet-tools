// Copyright (c) 2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if NETSTANDARD1_5
using System.Runtime.Loader;
#endif

namespace Saritasa.Tools.Messages.TestRuns.Internal
{
    /// <summary>
    /// Load types from assemblies. Also allows to load standard .NET types.
    /// </summary>
    internal static class TypeHelpers
    {
        /// <summary>
        /// Load type from type full name. Searchs for assemblies.
        /// </summary>
        /// <param name="fullName">Full type name.</param>
        /// <param name="assemblies">Assebmlies list. If null currently loaded assemblies would try to be searched.</param>
        /// <returns>Type. Null if type cannot be found.</returns>
        internal static Type LoadType(string fullName, Assembly[] assemblies = null)
        {
            Type t;

            if (assemblies == null)
            {
#if NETSTANDARD1_5
                assemblies = TypeHelpers.LoadAssembliesFromTypeName(fullName).ToArray();
#else
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif
            }

            // If it is a system type try to use Type.GetType first.
            if (fullName.StartsWith("System"))
            {
                t = Type.GetType(fullName, false, true);
                if (t != null)
                {
                    return t;
                }
            }

            // Then try to load from assemblies.
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                t = assembly.GetType(fullName, false, true);
                if (t != null)
                {
                    return t;
                }
            }

            // Last chance.
            return Type.GetType(fullName, false, true);
        }

        internal static IEnumerable<Assembly> LoadAssembliesFromTypeName(string typeName)
        {
            var assemblies = new List<Assembly>();
            var nsitems = typeName.Split('.');
            if (nsitems.Length < 1)
            {
                return assemblies;
            }
            var currentAssemblyName = nsitems[0];
            for (int i = 1; i < nsitems.Length; i++)
            {
                Assembly assembly = null;
                try
                {
#if NETSTANDARD1_5
                    assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(currentAssemblyName));
#else
                    assembly = Assembly.Load(currentAssemblyName);
#endif
                }
                catch (Exception)
                {
                    // Skip since it is possible that assembly with given name is not found.
                }

                if (assembly != null)
                {
                    assemblies.Add(assembly);
                }
                currentAssemblyName += "." + nsitems[i];
            }
            return assemblies;
        }
    }
}
