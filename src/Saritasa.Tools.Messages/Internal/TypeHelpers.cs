// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.ComponentModel;
#if NETSTANDARD1_5
using System.Runtime.Loader;
#endif

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// Load types from assemblies. Also allows to load standard .NET types.
    /// </summary>
    internal static class TypeHelpers
    {
        internal static object ResolveObjectForType(Type type, Func<Type, object> resolver, string loggingSource = "")
        {
            object obj = null;

            var typeInfo = type.GetTypeInfo();

            // Try default parameterless ctor.
            var ctor = typeInfo.GetConstructor(new Type[] { });
            if (ctor != null)
            {
                obj = ctor.Invoke(null);
            }

            // Try another ctor.
            if (obj == null)
            {
                var ctors = typeInfo.GetConstructors(BindingFlags.Public | BindingFlags.Instance |
                    BindingFlags.FlattenHierarchy);
                if (ctors.Length > 0)
                {
                    ctor = ctors[0];
                }
                else
                {
                    return null;
                }
                var ctorparams = ctor.GetParameters();
                var ctorparamsValues = new object[ctorparams.Length];
                for (int i = 0; i < ctorparams.Length; i++)
                {
                    ctorparamsValues[i] = resolver(ctorparams[i].ParameterType);
                }
                obj = ctor.Invoke(ctorparamsValues);
            }

            // Prefill public dependencies.
            if (obj != null)
            {
                var props = obj.GetType().GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance |
                    BindingFlags.FlattenHierarchy);
                for (var i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    if (prop.GetValue(obj) != null)
                    {
                        continue;
                    }

                    try
                    {
                        prop.SetValue(obj, resolver(prop.PropertyType));
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Error($"Cannot set value for handler {obj} of type {type}. {ex}.",
                            loggingSource);
                    }
                }
            }

            return obj;
        }

        /// <summary>
        /// Converts obj to type.
        /// </summary>
        /// <param name="obj">Initial object to convert.</param>
        /// <param name="type">Type to convert.</param>
        /// <returns>Converted object.</returns>
        internal static object ConvertType(object obj, Type type)
        {
            if (obj == null)
            {
                return null;
            }

#if !NETCOREAPP1_1 && !NETSTANDARD1_6
            var tc = TypeDescriptor.GetConverter(type);
            return tc.ConvertFrom(obj.ToString());
#else
            return obj;
#endif
        }

        private static readonly ConcurrentDictionary<Type, string> typeNamesDictionaryCache =
            new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Get partially type assembly qualified name. For example
        /// System.Globalization.NumberFormatInfo, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089 to
        /// System.Globalization.NumberFormatInfo, mscorlib .
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Partial assembly qualified name.</returns>
        internal static string GetPartiallyAssemblyQualifiedName(Type t)
        {
            if (t == null)
            {
                return String.Empty;
            }
            return typeNamesDictionaryCache.GetOrAdd(t, type =>
            {
                var aqn = type.AssemblyQualifiedName;
                var firstCommaIndex = aqn.IndexOf(',');
                var secondCommaIndex = aqn.IndexOf(',', firstCommaIndex + 1);
                return aqn.Substring(0, secondCommaIndex);
            });
        }
    }
}
