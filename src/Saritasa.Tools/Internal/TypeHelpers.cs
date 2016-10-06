// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace Saritasa.Tools.Internal
{
    using System;
    using System.Reflection;
    using Messages;
    using System.ComponentModel;

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
                t = Type.GetType(fullName, false, true);
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

            // for events we don't specify actual type
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

            // try default paramless ctor
            var ctor = typeInfo.GetConstructor(new Type[] { });
            if (ctor != null)
            {
                obj = ctor.Invoke(null);
            }

            // try another ctor
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

            // prefill public dependencies
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

        /// <summary>
        /// Converts obj to type.
        /// </summary>
        /// <param name="obj">Initial object to convert.</param>
        /// <param name="type">Type to convert.</param>
        /// <returns>Converted object.</returns>
        internal static object ConvertType(object obj, Type type)
        {
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
            var tc = TypeDescriptor.GetConverter(type);
            return tc.ConvertFrom(obj);
#else
            return obj;
#endif
        }
    }
}
