// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
#if NET40 || NETSTANDARD1_6_OR_GREATER || NET5_0_OR_GREATER
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
#endif
using System.Linq;
using System.Reflection;

namespace Saritasa.Tools.Common.Utils;

/// <summary>
/// Enum utils.
/// </summary>
public static class EnumUtils
{
#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Splits intercapped string.
    /// </summary>
    /// <example>TestDBConnection will be parsed into "Test", "DB", "Connection" parts.</example>
    private static readonly Regex intercappedStringSplitRegex =
        new Regex(@"((?<=[a-z])([A-Z])|(?<=[A-Z])([A-Z][a-z]))", RegexOptions.Compiled);

    /// <summary>
    /// Gets a description of enum value. If <see cref="DescriptionAttribute" /> is specified for it, its value will be returned.
    /// Instead split with spaces enums text will be returned (for example "IsActive" will be transformed to "Is Active").
    /// </summary>
    /// <param name="target">Enum value.</param>
    /// <returns>Description of the value.</returns>
    public static string GetDescription(Enum target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        var descAttribute = GetAttribute<DescriptionAttribute>(target);
        if (descAttribute != null)
        {
            return descAttribute.Description;
        }

        var value = target.ToString();

        // Split the value with spaces if it is intercapped.
        return intercappedStringSplitRegex.Replace(value, " $1");
    }
#endif

    /// <summary>
    /// Gets the custom attribute of enum value.
    /// </summary>
    /// <param name="target">Enum.</param>
    /// <typeparam name="TAttribute">Attribute type.</typeparam>
    /// <returns>Attribute or null if not found.</returns>
    public static TAttribute? GetAttribute<TAttribute>(Enum target)
        where TAttribute : Attribute
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

#if NET40
        FieldInfo? fieldInfo = target.GetType().GetField(target.ToString());
#else
        FieldInfo? fieldInfo = target.GetType().GetTypeInfo().GetDeclaredField(target.ToString());
#endif
        if (fieldInfo == null)
        {
            return null;
        }

#if NETSTANDARD1_6_OR_GREATER || NET5_0_OR_GREATER
        var attributes = fieldInfo.GetCustomAttributes<TAttribute>(false);
#else
        var attributes =
            (IEnumerable<TAttribute>)fieldInfo.GetCustomAttributes(typeof(TAttribute), false);
#endif
        return attributes.FirstOrDefault();
    }

    /// <summary>
    /// Get enumerable values.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <returns>Enumerable values.</returns>
    public static T[] GetValues<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<T>().ToArray();

#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Get the dictionary of enum name and enum description.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <returns>Key value pairs of enum name and its description.</returns>
    public static IDictionary<string, string> GetNamesWithDescriptions<T>() where T : Enum
        => GetValues<T>().ToDictionary(e => e.ToString(), e => GetDescription(e));

    /// <summary>
    /// Get the enumerable of key value pairs of enum name and enum description.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <returns>Key value pairs of enum name and its description.</returns>
    public static IDictionary<string, string> GetValuesWithDescriptions<T>() where T : Enum
        => GetValues<T>().ToDictionary(e => e.ToString("d"), e => GetDescription(e));
#endif
}
