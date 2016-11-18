// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// String class extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Formats target string with number of arguments. Equivalent of String.Format.
        /// </summary>
        /// <param name="format">Target string.</param>
        /// <param name="arg0">Argument 1.</param>
        /// <returns>Formatted string.</returns>
        [DebuggerStepThrough]
        public static string FormatWith(this string format, object arg0)
        {
            return string.Format(format, arg0);
        }

        /// <summary>
        /// Formats target string with number of arguments. Equivalent of String.Format.
        /// </summary>
        /// <param name="format">Target string.</param>
        /// <param name="arg0">Argument 1.</param>
        /// <param name="arg1">Argument 2.</param>
        /// <returns>Formatted string.</returns>
        [DebuggerStepThrough]
        public static string FormatWith(this string format, object arg0, object arg1)
        {
            return string.Format(format, arg0, arg1);
        }

        /// <summary>
        /// Formats target string with number of arguments. Equivalent of String.Format.
        /// </summary>
        /// <param name="format">Target string.</param>
        /// <param name="arg0">Argument 1.</param>
        /// <param name="arg1">Argument 2.</param>
        /// <param name="arg2">Argument 3.</param>
        /// <returns>Formatted string.</returns>
        [DebuggerStepThrough]
        public static string FormatWith(this string format, object arg0, object arg1, object arg2)
        {
            return string.Format(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// Formats target string with number of arguments. Equivalent of String.Format.
        /// </summary>
        /// <param name="format">Target string.</param>
        /// <param name="args">Arguments.</param>
        /// <returns>Formatted string.</returns>
        [DebuggerStepThrough]
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        /// <summary>
        /// Formats target string with number of arguments. Equivalent of String.Format.
        /// </summary>
        /// <param name="format">Target string.</param>
        /// <param name="provider">Format provider.</param>
        /// <param name="args">Arguments.</param>
        /// <returns>Formatted string.</returns>
        [DebuggerStepThrough]
        public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
        {
            return string.Format(provider, format, args);
        }

        /// <summary>
        /// Checks that target string is null or empty.
        /// </summary>
        /// <param name="target">Target string.</param>
        /// <returns>True if empty. False otherwise.</returns>
        [DebuggerStepThrough]
        public static bool IsEmpty(this string target)
        {
            return string.IsNullOrEmpty(target);
        }

        /// <summary>
        /// Checks that target string is not null or empty.
        /// </summary>
        /// <param name="target">Target string.</param>
        /// <returns>True if not empty. False otherwise.</returns>
        [DebuggerStepThrough]
        public static bool IsNotEmpty(this string target)
        {
            return !IsEmpty(target);
        }

        /// <summary>
        /// Returns empty string if target string is null or string itself.
        /// </summary>
        /// <param name="target">Target string.</param>
        /// <returns>Empty string if null or target string.</returns>
        [DebuggerStepThrough]
        public static string NullSafe(this string target)
        {
            return target ?? string.Empty;
        }
    }
}
