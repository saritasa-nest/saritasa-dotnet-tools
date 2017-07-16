// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using Saritasa.Tools.Messages.Common;
#if NETCOREAPP1_1 || NETSTANDARD1_6
using System.Runtime.Loader;
#endif

namespace Saritasa.Tools.Messages.Internal
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
#if NETCOREAPP1_1 || NETSTANDARD1_6
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

        /// <summary>
        /// Resolve type for content type.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="bytes">Message body (content).</param>
        /// <param name="objectSerializer">Serializer to be used.</param>
        /// <param name="assemblies">Assemblies that will be additionaly used for resolving.</param>
        internal static void ResolveTypeForContent(Message message, byte[] bytes, IObjectSerializer objectSerializer,
            Assembly[] assemblies)
        {
            if (bytes == null || bytes.Length < 1)
            {
                return;
            }

            // For events we don't specify actual type.
            if (message.Type == Message.MessageTypeQuery)
            {
                message.Content = objectSerializer.Deserialize(bytes, typeof(IDictionary<string, object>));
            }
            else
            {
                var t = LoadType(message.ContentType, assemblies);
                if (t != null)
                {
                    message.Content = objectSerializer.Deserialize(bytes, t);
                }
            }
        }

        /// <summary>
        /// The method fills message.Error property based on type message.ErrorType .
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="bytes">Error body.</param>
        /// <param name="objectSerializer">Serializer to use.</param>
        /// <param name="assemblies">Assemblies to search type.</param>
        internal static void ResolveTypeForError(Message message, byte[] bytes, IObjectSerializer objectSerializer,
            Assembly[] assemblies)
        {
            if (bytes == null || bytes.Length < 1)
            {
                return;
            }

            var t = LoadType(message.ErrorType, assemblies) ?? typeof(Exception);
            message.Error = (Exception)objectSerializer.Deserialize(bytes, t);
        }

        internal static object ResolveObjectForType(Type type, Func<Type, object> resolver, string loggingSource = "")
        {
            object obj = null;
            try
            {
                obj = resolver(type);
            }
            catch (Exception ex)
            {
                InternalLogger.Info($"Error while resolving type {type}: {ex}. Continue with fallback method.", loggingSource);
            }
            if (obj != null)
            {
                return obj;
            }

            var typeInfo = type.GetTypeInfo();

            // Try default paramless ctor.
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
                        InternalLogger.Error($"Cannot set value for handler {obj} of type {type}. {ex}",
                            loggingSource);
                    }
                }
            }

            return obj;
        }

        internal static void ResolveForParameters(object[] parameterValues, ParameterInfo[] parameters,
            Func<Type, object> resolver, string loggingSource = "")
        {
            if (parameterValues.Length != parameters.Length)
            {
                InternalLogger.Warn("Provided parameters count does not match method parameters count", loggingSource);
                return;
            }

            for (int i = 0; i < parameterValues.Length; i++)
            {
                if (parameterValues[i] != null)
                {
                    continue;
                }

                var type = parameters[i].ParameterType;
                try
                {
                    parameterValues[i] = resolver(type);
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn($"Cannot resolve parameter of type {type}: {ex}", loggingSource);
                }
            }
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
#if NETCOREAPP1_1 || NETSTANDARD1_6
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
