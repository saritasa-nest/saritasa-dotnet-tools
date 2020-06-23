// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Saritasa.Tools.Common.Utils
{
#if NETSTANDARD2_0 || NETSTANDARD2_1
    /// <summary>
    /// The class creates other object types with dependency resolving. Also has helpful methods
    /// to call object methods.
    /// </summary>
    public static class ObjectUtils
    {
        /// <summary>
        /// Create an object of the specified type with dependency resolving.
        /// </summary>
        /// <typeparam name="T">Object type to create.</typeparam>
        /// <param name="serviceProvider">Service provider.</param>
        /// <returns>Object or null.</returns>
        public static T CreateInstanceWithServiceProvider<T>(IServiceProvider serviceProvider) =>
            (T)CreateInstanceWithServiceProvider(typeof(T), serviceProvider);

        /// <summary>
        /// Create an object of the specified type with dependency resolving.
        /// </summary>
        /// <param name="type">Object type.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <returns>Object or null.</returns>
        public static object CreateInstanceWithServiceProvider(Type type, IServiceProvider serviceProvider)
        {
            // Find most descriptive constructor.
            var ctor = type
                .GetTypeInfo()
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance |
                    BindingFlags.FlattenHierarchy)
                .OrderByDescending(x => x.GetParameters().Length)
                .FirstOrDefault();
            if (ctor == null)
            {
                throw new InvalidOperationException(string.Format(Properties.Strings.NoSuitableConstructor, type.Name));
            }

            var inputParamObjects = ResolveParameters(ctor.GetParameters(), null, serviceProvider);
            return ctor.Invoke(inputParamObjects.ToArray());
        }

        /// <summary>
        /// The method resolves <c>parameters</c> using arguments and service providers. At first, the method tries to
        /// find value in args. Then, if it is a value type or string and has default value the value will be a default. Finally,
        /// it tries to use a service provider.
        /// </summary>
        /// <param name="parameters">Parameters to resolve.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <returns>Resolved parameters.</returns>
        internal static object?[] ResolveParameters(
            ParameterInfo[] parameters,
            IDictionary<string, object>? args = null,
            IServiceProvider? serviceProvider = null)
        {
            var inputParams = new object?[parameters.Length];
            int paramIndex = 0;
            foreach (ParameterInfo parameterInfo in parameters)
            {
                object? value = null;
                if (args != null)
                {
                    var nameKey = (parameterInfo.Name ?? string.Empty).ToLower();
                    if (!string.IsNullOrEmpty(nameKey) && args.ContainsKey(nameKey))
                    {
                        var arg = args[nameKey];
                        // Try to convert argument if needed.
                        if (parameterInfo.ParameterType.IsAssignableFrom(arg.GetType()))
                        {
                            value = arg;
                        }
                        else
                        {
                            var tc = TypeDescriptor.GetConverter(parameterInfo.ParameterType);
                            value = tc.ConvertFrom(args[nameKey]);
                        }
                    }
                }
                else if ((parameterInfo.ParameterType.IsValueType || parameterInfo.ParameterType == typeof(string)) &&
                    parameterInfo.HasDefaultValue)
                {
                    value = parameterInfo.DefaultValue;
                }
                else if (serviceProvider != null)
                {
                    value = serviceProvider.GetService(parameterInfo.ParameterType);
                }
                inputParams[paramIndex++] = value;
            }

            return inputParams;
        }
    }
#endif
}
