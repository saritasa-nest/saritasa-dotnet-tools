// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET40 || NET452 || NET461
using System.Collections.Generic;
using System.ComponentModel;
#endif
using System.Linq;
using System.Reflection;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Enum utils.
    /// </summary>
    public static class EnumUtils
    {
#if NET40 || NET452 || NET461
        /// <summary>
        /// Gets the value of Description attribute.
        /// </summary>
        /// <param name="target">Enum.</param>
        /// <returns>Description text.</returns>
        public static string GetDescription(Enum target)
        {
            var descAttribute = GetAttribute<DescriptionAttribute>(target);
            if (descAttribute == null)
            {
                return target.ToString();
            }

            return descAttribute.Description;
        }
#endif

        /// <summary>
        /// Gets the custom attribute of enum value.
        /// </summary>
        /// <param name="target">Enum.</param>
        /// <typeparam name="TAttribute">Attribute type.</typeparam>
        /// <returns>Attribute or null if not found.</returns>
        public static TAttribute GetAttribute<TAttribute>(Enum target)
            where TAttribute : Attribute
        {
#if NET40 || NET452 || NET461
            if (!target.GetType().IsEnum)
            {
                throw new ArgumentOutOfRangeException(nameof(target), Properties.Strings.ArgumentMustBeEnum);
            }
#endif

#if NET40 || NET452 || NET461
            FieldInfo fieldInfo = target.GetType().GetField(target.ToString());
#else
            FieldInfo fieldInfo = target.GetType().GetTypeInfo().GetDeclaredField(target.ToString());
#endif
            if (fieldInfo == null)
            {
                return null;
            }

#if NETSTANDARD1_2
            var attributes = fieldInfo.GetCustomAttributes<TAttribute>(false);
#else
            var attributes =
                (IEnumerable<TAttribute>)fieldInfo.GetCustomAttributes(typeof(TAttribute), false);
#endif
            return attributes.FirstOrDefault();
        }
    }
}
