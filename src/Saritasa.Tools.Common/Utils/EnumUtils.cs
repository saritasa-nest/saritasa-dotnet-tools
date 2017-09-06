// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET40 || NET452 || NET461
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
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
        /// Splits intercapped string.
        /// </summary>
        /// <example>TestDBConnection will be parsed into "Test", "DB", "Connection" parts.</example>
        private static readonly Regex IntercappedStringSplitRegex = new Regex(@"((?<=[a-z])([A-Z])|(?<=[A-Z])([A-Z][a-z]))", RegexOptions.Compiled);

        /// <summary>
        /// Gets a description of enum value. If <see cref="DescriptionAttribute"/> is specified for it, its value will be returned.
        /// </summary>
        /// <param name="target">Enum value.</param>
        /// <returns>Description of the value.</returns>
        public static string GetDescription(Enum target)
        {
            var descAttribute = GetAttribute<DescriptionAttribute>(target);
            if (descAttribute != null)
            {
                return descAttribute.Description;
            }

            var value = target.ToString();

            // Split the value with spaces if it is intercapped.
            return IntercappedStringSplitRegex.Replace(value, " $1");
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
