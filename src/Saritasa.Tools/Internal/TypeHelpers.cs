// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Internal
{
    using System;
    using System.Reflection;
    using Messages;

    /// <summary>
    /// Load types from assemblies. Also allows to load standard .NET types.
    /// </summary>
    public static class TypeHelpers
    {
        /// <summary>
        /// Load type from type full name. Searchs for assemblies.
        /// </summary>
        /// <param name="fullName">Full type name.</param>
        /// <param name="assemblies">Assebmlies list.</param>
        /// <returns>Type. Null if type cannot be found.</returns>
        public static Type LoadType(string fullName, Assembly[] assemblies)
        {
            Type t = null;

            // if it is a system type try to use Type.GetType first
            if (fullName.StartsWith("System"))
            {
                Type.GetType(fullName, false, true);
                if (t != null)
                {
                    return t;
                }
            }

            // then try to load from assemblies
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                t = assembly.GetType(fullName, false, true);
                if (t != null)
                {
                    return t;
                }
            }

            // last chance
            Type.GetType(fullName, false, true);
            if (t != null)
            {
                return t;
            }

            return null;
        }

        internal static void ResolveTypeForContent(Message message, byte[] bytes, IObjectSerializer objectSerializer, Assembly[] assemblies)
        {
            if (bytes == null || bytes.Length < 1)
            {
                return;
            }

            var t = LoadType(message.ContentType, assemblies);
            if (t != null)
            {
                message.Content = objectSerializer.Deserialize(bytes, t);
            }
        }

        internal static void ResolveTypeForError(Message message, byte[] bytes, IObjectSerializer objectSerializer, Assembly[] assemblies)
        {
            if (bytes == null || bytes.Length < 1)
            {
                return;
            }

            var t = LoadType(message.ContentType, assemblies);
            if (t == null)
            {
                t = typeof(Exception);
            }

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

            // try default paramless ctor
            var ctor = type.GetTypeInfo().GetConstructor(new Type[] { });
            if (ctor != null)
            {
                obj = ctor.Invoke(null);
            }

            // try another ctor
            if (obj == null)
            {
                var ctors = type.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance |
                    BindingFlags.FlattenHierarchy);
                if (ctors.Length > 0)
                {
                    ctor = ctors[0];
                }
                var ctorparams = ctor.GetParameters();
                object[] ctorparamsValues = new object[ctorparams.Length];
                for (int i = 0; i < ctorparams.Length; i++)
                {
                    ctorparamsValues[i] = resolver(ctorparams[i].ParameterType);
                }
                obj = ctor.Invoke(ctorparamsValues);
            }

            // prefill public dependencies
            if (obj != null)
            {
                var props = obj.GetType().GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance |
                    BindingFlags.FlattenHierarchy);
                foreach (var prop in props)
                {
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
                InternalLogger.Warn($"Provided parameters count does not match method parameters count", loggingSource);
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
    }
}
