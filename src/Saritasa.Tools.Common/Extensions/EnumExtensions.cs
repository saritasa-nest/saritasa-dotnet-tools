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
            if (!target.GetType().IsEnum)
            {
                throw new ArgumentOutOfRangeException(nameof(target), "Target is not enum");
            }

            FieldInfo fieldInfo = target.GetType().GetField(target.ToString());
            if (fieldInfo == null)
            {
                return null;
            }

            IEnumerable<DescriptionAttribute> attributes = fieldInfo.GetCustomAttributes<DescriptionAttribute>(false);
            return attributes.Any() ? attributes.First().Description : target.ToString();
        }
#endif
    }
}
