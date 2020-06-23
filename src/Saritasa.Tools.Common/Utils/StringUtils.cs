// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Saritasa.Tools.Common.Utils
{
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
        public static string SafeTruncate(string target, int maxLength)
        {
            Guard.IsNotNegativeOrZero(maxLength, nameof(maxLength));
            return target.Length <= maxLength ? target : target.Substring(0, maxLength);
        }

        /// <summary>
        /// Joins the objects ignore empty ones.
        /// </summary>
        /// <param name="separator">The string to use as a separator.</param>
        /// <param name="values">The values.</param>
        /// <returns>Concatenated string.</returns>
        public static string JoinIgnoreEmpty(string separator, params string[] values)
        {
            Guard.IsNotEmpty(separator, nameof(separator));
            Guard.IsNotNull(values, nameof(values));
            return string.Join(separator, values.Where(x => !string.IsNullOrEmpty(x)));
        }

        /// <summary>
        /// Joins the strings ignore empty ones.
        /// </summary>
        /// <param name="separator">The string to use as a separator.</param>
        /// <param name="values">The values.</param>
        /// <returns>Concatenated string.</returns>
        public static string JoinIgnoreEmpty(string separator, IEnumerable<string> values)
        {
            Guard.IsNotEmpty(separator, nameof(separator));
            Guard.IsNotNull(values, nameof(values));
            return string.Join(separator, values.Where(x => !string.IsNullOrEmpty(x)));
        }

        /// <summary>
        /// Retrieves a substring from this instance. If the start index has negative value it will be replaced
        /// to 0. If substring exceeds the length of the target string the end of the string will be returned. <c>null</c> will
        /// be converted to the empty string.
        /// </summary>
        /// <param name="target">Target string.</param>
        /// <param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>Substring.</returns>
        public static string SafeSubstring(string target, int startIndex, int length = 0)
        {
            if (target == null)
            {
                return string.Empty;
            }

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
        /// Returns empty string if target string is null or string itself.
        /// </summary>
        /// <param name="target">Target string.</param>
        /// <returns>Empty string if null or target string.</returns>
        public static string NullSafe(string target)
        {
            return target ?? string.Empty;
        }

        /// <summary>
        /// Capitalize the return word if the parameter is not capitalized.
        /// For example if word is "Table", then returns "Tables".
        /// The method skips empty or null strings.
        /// </summary>
        /// <param name="target">Target word.</param>
        /// <returns>Capitalized target.</returns>
        public static string Capitalize(string target)
        {
            if (string.IsNullOrEmpty(target) || char.IsUpper(target, 0))
            {
                return target;
            }
            if (target.Length == 0)
            {
                return target;
            }
            var stringBuilder = new StringBuilder(target.Length);
            stringBuilder.Append(char.ToUpperInvariant(target[0]));
            stringBuilder.Append(target.Substring(1));
            return stringBuilder.ToString();
        }

        #region Parse with default

        /// <summary>
        /// Tries to convert the target string to Boolean. If fails return the default value.
        /// </summary>
        public static Boolean ParseOrDefault(string target, Boolean defaultValue)
        {
            Boolean result;
            var success = Boolean.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        private static readonly string[] trueValues = { "yes", "on", "y", "t", "1" };
        private static readonly string[] falseValues = { "no", "off", "n", "f", "0" };

        /// <summary>
        /// Tries to convert the target string to Boolean. If fails return the default value.
        /// SetPart extended parameter to true to be able to parse from values "0", "1", "Yes", "No".
        /// </summary>
        public static Boolean ParseOrDefault(string target, Boolean defaultValue, Boolean extended)
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
        /// Tries to convert the target string to Byte. If fails return the default value.
        /// </summary>
        public static Byte ParseOrDefault(string target, Byte defaultValue)
        {
            Byte result;
            var success = Byte.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Byte. If fails return the default value.
        /// </summary>
        public static Byte ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, Byte defaultValue)
        {
            Byte result;
            var success = Byte.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Char. If fails return the default value.
        /// </summary>
        public static Char ParseOrDefault(string target, Char defaultValue)
        {
            Char result;
            var success = Char.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to DateTime. If fails return the default value.
        /// </summary>
        public static DateTime ParseOrDefault(string target, DateTime defaultValue)
        {
            DateTime result;
            var success = DateTime.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to DateTime. If fails return the default value.
        /// </summary>
        public static DateTime ParseOrDefault(string target, IFormatProvider provider, DateTimeStyles styles, DateTime defaultValue)
        {
            DateTime result;
            var success = DateTime.TryParse(target, provider, styles, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Decimal. If fails return the default value.
        /// </summary>
        public static Decimal ParseOrDefault(string target, Decimal defaultValue)
        {
            Decimal result;
            var success = Decimal.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Decimal. If fails return the default value.
        /// </summary>
        public static Decimal ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, Decimal defaultValue)
        {
            Decimal result;
            var success = Decimal.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Double. If fails return the default value.
        /// </summary>
        public static Double ParseOrDefault(string target, Double defaultValue)
        {
            Double result;
            var success = Double.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Double. If fails return the default value.
        /// </summary>
        public static Double ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, Double defaultValue)
        {
            Double result;
            var success = Double.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Int16. If fails return the default value.
        /// </summary>
        public static Int16 ParseOrDefault(string target, Int16 defaultValue)
        {
            Int16 result;
            var success = Int16.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Int16. If fails return the default value.
        /// </summary>
        public static Int16 ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, Int16 defaultValue)
        {
            Int16 result;
            var success = Int16.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to int. If fails return the default value.
        /// </summary>
        public static Int32 ParseOrDefault(string target, Int32 defaultValue)
        {
            Int32 result;
            var success = Int32.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to int. If fails return the default value.
        /// </summary>
        public static Int32 ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, Int32 defaultValue)
        {
            Int32 result;
            var success = Int32.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Int64. If fails return the default value.
        /// </summary>
        public static Int64 ParseOrDefault(string target, Int64 defaultValue)
        {
            Int64 result;
            var success = Int64.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Int64. If fails return the default value.
        /// </summary>
        public static Int64 ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, Int64 defaultValue)
        {
            Int64 result;
            var success = Int64.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to SByte. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static SByte ParseOrDefault(string target, SByte defaultValue)
        {
            SByte result;
            var success = SByte.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to SByte. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static SByte ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, SByte defaultValue)
        {
            SByte result;
            var success = SByte.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Single. If fails return the default value.
        /// </summary>
        public static Single ParseOrDefault(string target, Single defaultValue)
        {
            Single result;
            var success = Single.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Single. If fails return the default value.
        /// </summary>
        public static Double ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, Single defaultValue)
        {
            Single result;
            var success = Single.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to UInt16. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static UInt16 ParseOrDefault(string target, UInt16 defaultValue)
        {
            UInt16 result;
            var success = UInt16.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to UInt16. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static UInt16 ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, UInt16 defaultValue)
        {
            UInt16 result;
            var success = UInt16.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Uint. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static UInt32 ParseOrDefault(string target, UInt32 defaultValue)
        {
            UInt32 result;
            var success = UInt32.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Uint. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static UInt32 ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, UInt32 defaultValue)
        {
            UInt32 result;
            var success = UInt32.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to UInt64. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static UInt64 ParseOrDefault(string target, UInt64 defaultValue)
        {
            UInt64 result;
            var success = UInt64.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to UInt64. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static UInt64 ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, UInt64 defaultValue)
        {
            UInt64 result;
            var success = UInt64.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Enum. If fails return the default value.
        /// </summary>
        public static T ParseOrDefault<T>(string target, T defaultValue) where T : struct
        {
            return Enum.IsDefined(typeof(T), target) ? (T)Enum.Parse(typeof(T), target, true) : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Enum. If fails return the default value.
        /// </summary>
        public static T ParseOrDefault<T>(string target, Boolean ignoreCase, T defaultValue) where T : struct
        {
            return Enum.IsDefined(typeof(T), target) ? (T)Enum.Parse(typeof(T), target, ignoreCase) : defaultValue;
        }

        #endregion
    }
}
