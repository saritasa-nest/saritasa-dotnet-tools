// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using JetBrains.Annotations;
    using Extensions;

    /// <summary>
    /// Strings utils.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Truncates string to max length.
        /// </summary>
        /// <param name="target">Target string.</param>
        /// <param name="maxLength">Max length.</param>
        /// <returns>Truncated string.</returns>
        [DebuggerStepThrough]
        public static string Truncate([NotNull] string target, int maxLength)
        {
            Guard.IsNotEmpty(target, nameof(target));
            Guard.IsNotNegativeOrZero(maxLength, nameof(maxLength));
            return target.Length <= maxLength ? target : target.Substring(0, maxLength);
        }

        /// <summary>
        /// Joins the objects ignore empty ones.
        /// </summary>
        /// <param name="separator">The string to use as a separator.</param>
        /// <param name="values">The values.</param>
        /// <returns>Concatenated string.</returns>
        [DebuggerStepThrough]
        public static string JoinIgnoreEmpty([NotNull] string separator, params string[] values)
        {
            Guard.IsNotEmpty(separator, nameof(separator));
            Guard.IsNotNull(values, nameof(values));
            return string.Join(separator, values.Where(x => x.IsNotEmpty()));
        }

        /// <summary>
        /// Joins the strings ignore empty ones.
        /// </summary>
        /// <param name="separator">The string to use as a separator.</param>
        /// <param name="values">The values.</param>
        /// <returns>Concatenated string.</returns>
        [DebuggerStepThrough]
        public static string JoinIgnoreEmpty([NotNull] string separator, [NotNull] IEnumerable<string> values)
        {
            Guard.IsNotEmpty(separator, nameof(separator));
            Guard.IsNotNull(values, nameof(values));
            return string.Join(separator, values.Where(x => x.IsNotEmpty()));
        }

        /// <summary>
        /// Converts wildcards to regex. Determines what reg exp correspond to string with * and ? chars.
        /// </summary>
        /// <param name="pattern">The wildcards pattern.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string WildcardToRegex([NotNull] string pattern)
        {
            Guard.IsNotEmpty(pattern, nameof(pattern));
            return ("^" + Regex.Escape(pattern)).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        }

        /// <summary>
        /// Reverse string characters. "123" -> "321".
        /// </summary>
        /// <param name="target">Target string.</param>
        /// <returns>Reversed string.</returns>
        [DebuggerStepThrough]
        public static string Reverse([NotNull] string target)
        {
            Guard.IsNotEmpty(target, nameof(target));

            char[] arr = target.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /// <summary>
        /// Retrieves a substring from this instance. If start index had negative value it will be replaced
        /// to 0. If substring exceed length of target string the end of string will be returned.
        /// </summary>
        /// <param name="target">Target string.</param>
        /// <param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>Substring.</returns>
        [DebuggerStepThrough]
        public static string SafeSubstring([NotNull] string target, int startIndex, int length = 0)
        {
            Guard.IsNotEmpty(target, nameof(target));

            if (startIndex < 0)
            {
                startIndex = 0;
            }
            else if (startIndex > target.Length)
            {
                return string.Empty;
            }
            if (length == 0)
            {
                length = target.Length;
            }
            else if (startIndex + length > target.Length)
            {
                length = target.Length - startIndex;
            }
            return target.Substring(startIndex, length);
        }

        /// <summary>
        /// Converts the string to snake case style (HelloWorld -> Hello_World). The string will have underscore (_) in front of
        /// each upper case letter. The function does not remove spaces and does not make string lower case.
        /// </summary>
        public static string ConvertToSnakeCase([NotNull] string target)
        {
            return string.Concat(target.Select((ch, index) => index > 0 && char.IsUpper(ch) ? "_" + ch.ToString() : ch.ToString()).ToArray());
        }

        #region Parse with default

        /// <summary>
        /// Tries to convert target string to Boolean. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Boolean ParseDefault(string target, Boolean defaultValue)
        {
            Boolean result;
            var success = Boolean.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        private static readonly string[] trueValues = { "yes", "y", "t", "1" };
        private static readonly string[] falseValues = { "no", "n", "f", "0" };

        /// <summary>
        /// Tries to convert target string to Boolean. If fails returns default value.
        /// Set extended parameter to true to be able to parse from values "0", "1", "Yes", "No".
        /// </summary>
        [DebuggerStepThrough]
        public static Boolean ParseDefault(string target, Boolean defaultValue, Boolean extended)
        {
            Boolean result;
            var success = Boolean.TryParse(target, out result);
            if (extended && !success)
            {
                var trimmedTarget = target.ToLowerInvariant().Trim();
                if (Array.IndexOf(trueValues, trimmedTarget) > -1)
                {
                    success = true;
                    result = true;
                }
                else if (Array.IndexOf(falseValues, trimmedTarget) > -1)
                {
                    success = true;
                    result = false;
                }
            }
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Byte. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Byte ParseDefault(string target, Byte defaultValue)
        {
            Byte result;
            var success = Byte.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Byte. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Byte ParseDefault(string target, NumberStyles style, IFormatProvider provider, Byte defaultValue)
        {
            Byte result;
            var success = Byte.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Char. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Char ParseDefault(string target, Char defaultValue)
        {
            Char result;
            var success = Char.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to DateTime. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static DateTime ParseDefault(string target, DateTime defaultValue)
        {
            DateTime result;
            var success = DateTime.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to DateTime. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static DateTime ParseDefault(string target, IFormatProvider provider, DateTimeStyles styles, DateTime defaultValue)
        {
            DateTime result;
            var success = DateTime.TryParse(target, provider, styles, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Decimal. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Decimal ParseDefault(string target, Decimal defaultValue)
        {
            Decimal result;
            var success = Decimal.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Decimal. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Decimal ParseDefault(string target, NumberStyles style, IFormatProvider provider, Decimal defaultValue)
        {
            Decimal result;
            var success = Decimal.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Double. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Double ParseDefault(string target, Double defaultValue)
        {
            Double result;
            var success = Double.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Double. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Double ParseDefault(string target, NumberStyles style, IFormatProvider provider, Double defaultValue)
        {
            Double result;
            var success = Double.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Int16. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Int16 ParseDefault(string target, Int16 defaultValue)
        {
            Int16 result;
            var success = Int16.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Int16. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Int16 ParseDefault(string target, NumberStyles style, IFormatProvider provider, Int16 defaultValue)
        {
            Int16 result;
            var success = Int16.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to int. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static int ParseDefault(string target, int defaultValue)
        {
            int result;
            var success = int.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to int. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Int32 ParseDefault(string target, NumberStyles style, IFormatProvider provider, Int32 defaultValue)
        {
            Int32 result;
            var success = Int32.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Int64. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Int64 ParseDefault(string target, Int64 defaultValue)
        {
            Int64 result;
            var success = Int64.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Int64. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Int64 ParseDefault(string target, NumberStyles style, IFormatProvider provider, Int64 defaultValue)
        {
            Int64 result;
            var success = Int64.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to SByte. If fails returns default value.
        /// </summary>
        [CLSCompliant(false)]
        [DebuggerStepThrough]
        public static SByte ParseDefault(string target, SByte defaultValue)
        {
            SByte result;
            var success = SByte.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to SByte. If fails returns default value.
        /// </summary>
        [CLSCompliant(false)]
        [DebuggerStepThrough]
        public static SByte ParseDefault(string target, NumberStyles style, IFormatProvider provider, SByte defaultValue)
        {
            SByte result;
            var success = SByte.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Single. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Single ParseDefault(string target, Single defaultValue)
        {
            Single result;
            var success = Single.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Single. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static Double ParseDefault(string target, NumberStyles style, IFormatProvider provider, Single defaultValue)
        {
            Single result;
            var success = Single.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to UInt16. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        [CLSCompliant(false)]
        public static UInt16 ParseDefault(string target, UInt16 defaultValue)
        {
            UInt16 result;
            var success = UInt16.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to UInt16. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        [CLSCompliant(false)]
        public static UInt16 ParseDefault(string target, NumberStyles style, IFormatProvider provider, UInt16 defaultValue)
        {
            UInt16 result;
            var success = UInt16.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Uint. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        [CLSCompliant(false)]
        public static UInt32 ParseDefault(string target, UInt32 defaultValue)
        {
            UInt32 result;
            var success = UInt32.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Uint. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        [CLSCompliant(false)]
        public static UInt32 ParseDefault(string target, NumberStyles style, IFormatProvider provider, UInt32 defaultValue)
        {
            UInt32 result;
            var success = UInt32.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to UInt64. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        [CLSCompliant(false)]
        public static UInt64 ParseDefault(string target, UInt64 defaultValue)
        {
            UInt64 result;
            var success = UInt64.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to UInt64. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        [CLSCompliant(false)]
        public static UInt64 ParseDefault(string target, NumberStyles style, IFormatProvider provider, UInt64 defaultValue)
        {
            UInt64 result;
            var success = UInt64.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Enum. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static T ParseDefault<T>(string target, T defaultValue) where T : struct
        {
            return Enum.IsDefined(typeof(T), target) ? (T)Enum.Parse(typeof(T), target, true) : defaultValue;
        }

        /// <summary>
        /// Tries to convert target string to Enum. If fails returns default value.
        /// </summary>
        [DebuggerStepThrough]
        public static T ParseDefault<T>(string target, Boolean ignoreCase, T defaultValue) where T : struct
        {
            return Enum.IsDefined(typeof(T), target) ? (T)Enum.Parse(typeof(T), target, ignoreCase) : defaultValue;
        }

        #endregion
    }
}
