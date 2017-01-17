// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Linq;

    /// <summary>
    /// System.Enum extensions.
    /// </summary>
    public static class EnumExtensions
    {
#if !PORTABLE && !NETSTANDARD1_2 && !NETSTANDARD1_6 && !NETCOREAPP1_0 && !NETCOREAPP1_1
        /// <summary>
        /// Gets the value of Description attribute.
        /// </summary>
        /// <param name="target">Enum.</param>
        /// <returns>Description text.</returns>
        public static string GetDescription(this Enum target)
        {
            var descAttribute = GetAttribute<DescriptionAttribute>(target);
            return descAttribute?.Description;
        }
#endif

        /// <summary>
        /// Gets the custom attribute of enum.
        /// </summary>
        /// <param name="target">Enum.</param>
        /// <typeparam name="TAttribute">Attribute type.</typeparam>
        /// <returns>Attribute or null if not found.</returns>
        public static TAttribute GetAttribute<TAttribute>(this Enum target)
            where TAttribute : Attribute
        {
#if !PORTABLE && !NETSTANDARD1_2 && !NETSTANDARD1_6 && !NETCOREAPP1_0 && !NETCOREAPP1_1
            if (!target.GetType().IsEnum)
            {
                throw new ArgumentOutOfRangeException(nameof(target), "Target is not enum");
            }
#endif

#if !NETSTANDARD1_2 && !NETSTANDARD1_6
            FieldInfo fieldInfo = target.GetType().GetField(target.ToString());
#else
            FieldInfo fieldInfo = target.GetType().GetTypeInfo().GetDeclaredField(target.ToString());
#endif
            if (fieldInfo == null)
            {
                return null;
            }

#if !NET35 && !NET40
            var attributes = fieldInfo.GetCustomAttributes<TAttribute>(false);
#else
            var attributes =
                (IEnumerable<TAttribute>)fieldInfo.GetCustomAttributes(typeof(TAttribute), false);
#endif
            return attributes.FirstOrDefault();
        }
    }
}
