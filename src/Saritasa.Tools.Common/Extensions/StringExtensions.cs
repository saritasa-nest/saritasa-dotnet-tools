// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Saritasa.Tools.Common.Extensions
{
    /// <summary>
    /// <see cref="String" /> class extensions.
    /// </summary>
    public static class StringExtensions
    {
        #region FormatWith

        /// <summary>
        /// Formats target string with number of arguments. Equivalent of String.Format.
        /// </summary>
        /// <param name="format">Target string.</param>
        /// <param name="arg0">Argument 1.</param>
        /// <returns>Formatted string.</returns>
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
        public static string FormatWith(
            this string format,
            object arg0,
            object arg1)
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
        public static string FormatWith(
            this string format,
            object arg0,
            object arg1,
            object arg2)
        {
            return string.Format(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// Formats target string with number of arguments. Equivalent of String.Format.
        /// </summary>
        /// <param name="format">Target string.</param>
        /// <param name="args">Arguments.</param>
        /// <returns>Formatted string.</returns>
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
        public static string FormatWith(
            this string format,
            IFormatProvider provider,
            params object[] args)
        {
            return string.Format(provider, format, args);
        }

        #endregion
    }
}
