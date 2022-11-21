// Copyright (c) 2015-2022, Saritasa. All rights reserved.
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
        public static string NullSafe(string? target)
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

        #region Parse or default bool

        /// <summary>
        /// Tries to convert the target string to Boolean. If fails return the default value.
        /// </summary>
        public static bool ParseOrDefault(string target, bool defaultValue)
        {
            bool result;
            var success = bool.TryParse(target, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to Boolean. If fails return the default value.
        /// </summary>
        public static bool ParseOrDefault(ReadOnlySpan<char> target, bool defaultValue)
        {
            bool result;
            var success = bool.TryParse(target, out result);
            return success ? result : defaultValue;
        }
#endif

        private static readonly string[] trueValues = { "yes", "on", "y", "t", "1" };
        private static readonly string[] falseValues = { "no", "off", "n", "f", "0" };

        /// <summary>
        /// Tries to convert the target string to Boolean. If fails return the default value.
        /// The method also takes into account extended values (such as "0", "1", "Yes", "No") for conversion.
        /// </summary>
        public static bool ParseOrDefaultExtended(string target, bool defaultValue)
        {
            bool result;
            var success = bool.TryParse(target, out result);
            if (!success)
            {
                var trimmedTarget = target != null ? target.ToLower().Trim() : string.Empty;
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

        #endregion

        #region Parse or default byte

        /// <summary>
        /// Tries to convert the target string to Byte. If fails return the default value.
        /// </summary>
        public static byte ParseOrDefault(string target, byte defaultValue)
        {
            byte result;
            var success = byte.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Byte. If fails return the default value.
        /// </summary>
        public static byte ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, byte defaultValue)
        {
            byte result;
            var success = byte.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to Byte. If fails return the default value.
        /// </summary>
        public static byte ParseOrDefault(ReadOnlySpan<char> target, byte defaultValue)
        {
            byte result;
            var success = byte.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Byte. If fails return the default value.
        /// </summary>
        public static byte ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, byte defaultValue)
        {
            byte result;
            var success = byte.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default char

        /// <summary>
        /// Tries to convert the target string to Char. If fails return the default value.
        /// </summary>
        public static char ParseOrDefault(string target, char defaultValue)
        {
            char result;
            var success = char.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        #endregion

        #region Parse or default date time

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

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to DateTime. If fails return the default value.
        /// </summary>
        public static DateTime ParseOrDefault(ReadOnlySpan<char> target, DateTime defaultValue)
        {
            DateTime result;
            var success = DateTime.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to DateTime. If fails return the default value.
        /// </summary>
        public static DateTime ParseOrDefault(ReadOnlySpan<char> target, IFormatProvider provider, DateTimeStyles styles, DateTime defaultValue)
        {
            DateTime result;
            var success = DateTime.TryParse(target, provider, styles, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default date time offset

        /// <summary>
        /// Tries to convert the target string to <see cref="DateTimeOffset" />. If fails return the default value.
        /// </summary>
        public static DateTimeOffset ParseOrDefault(string target, DateTimeOffset defaultValue)
        {
            DateTimeOffset result;
            var success = DateTimeOffset.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to <see cref="DateTimeOffset" />. If fails return the default value.
        /// </summary>
        public static DateTimeOffset ParseOrDefault(string target, IFormatProvider provider, DateTimeStyles styles, DateTimeOffset defaultValue)
        {
            DateTimeOffset result;
            var success = DateTimeOffset.TryParse(target, provider, styles, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to <see cref="DateTimeOffset" />. If fails return the default value.
        /// </summary>
        public static DateTimeOffset ParseOrDefault(ReadOnlySpan<char> target, DateTimeOffset defaultValue)
        {
            DateTimeOffset result;
            var success = DateTimeOffset.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to <see cref="DateTimeOffset" />. If fails return the default value.
        /// </summary>
        public static DateTimeOffset ParseOrDefault(ReadOnlySpan<char> target, IFormatProvider provider, DateTimeStyles styles, DateTimeOffset defaultValue)
        {
            DateTimeOffset result;
            var success = DateTimeOffset.TryParse(target, provider, styles, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default date only

#if NET6_0_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to <see cref="DateOnly" />. If fails return the default value.
        /// </summary>
        public static DateOnly ParseOrDefault(string target, DateOnly defaultValue)
        {
            DateOnly result;
            var success = DateOnly.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to <see cref="DateOnly" />. If fails return the default value.
        /// </summary>
        public static DateOnly ParseOrDefault(string target, IFormatProvider provider, DateTimeStyles styles, DateOnly defaultValue)
        {
            DateOnly result;
            var success = DateOnly.TryParse(target, provider, styles, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to <see cref="DateOnly" />. If fails return the default value.
        /// </summary>
        public static DateOnly ParseOrDefault(ReadOnlySpan<char> target, DateOnly defaultValue)
        {
            DateOnly result;
            var success = DateOnly.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to <see cref="DateTimeOffset" />. If fails return the default value.
        /// </summary>
        public static DateOnly ParseOrDefault(ReadOnlySpan<char> target, IFormatProvider provider, DateTimeStyles styles, DateOnly defaultValue)
        {
            DateOnly result;
            var success = DateOnly.TryParse(target, provider, styles, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default decimal

        /// <summary>
        /// Tries to convert the target string to Decimal. If fails return the default value.
        /// </summary>
        public static decimal ParseOrDefault(string target, decimal defaultValue)
        {
            decimal result;
            var success = decimal.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Decimal. If fails return the default value.
        /// </summary>
        public static decimal ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, decimal defaultValue)
        {
            decimal result;
            var success = decimal.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to Decimal. If fails return the default value.
        /// </summary>
        public static decimal ParseOrDefault(ReadOnlySpan<char> target, decimal defaultValue)
        {
            decimal result;
            var success = decimal.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Decimal. If fails return the default value.
        /// </summary>
        public static decimal ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, decimal defaultValue)
        {
            decimal result;
            var success = decimal.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default double

        /// <summary>
        /// Tries to convert the target string to Double. If fails return the default value.
        /// </summary>
        public static double ParseOrDefault(string target, double defaultValue)
        {
            double result;
            var success = double.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Double. If fails return the default value.
        /// </summary>
        public static double ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, double defaultValue)
        {
            double result;
            var success = double.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to Double. If fails return the default value.
        /// </summary>
        public static double ParseOrDefault(ReadOnlySpan<char> target, double defaultValue)
        {
            double result;
            var success = double.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Double. If fails return the default value.
        /// </summary>
        public static double ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, double defaultValue)
        {
            double result;
            var success = double.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default short

        /// <summary>
        /// Tries to convert the target string to Int16. If fails return the default value.
        /// </summary>
        public static short ParseOrDefault(string target, short defaultValue)
        {
            short result;
            var success = short.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Int16. If fails return the default value.
        /// </summary>
        public static short ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, short defaultValue)
        {
            short result;
            var success = short.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to Int16. If fails return the default value.
        /// </summary>
        public static short ParseOrDefault(ReadOnlySpan<char> target, short defaultValue)
        {
            short result;
            var success = short.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Int16. If fails return the default value.
        /// </summary>
        public static short ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, short defaultValue)
        {
            short result;
            var success = short.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default int

        /// <summary>
        /// Tries to convert the target string to int. If fails return the default value.
        /// </summary>
        public static int ParseOrDefault(string target, int defaultValue)
        {
            int result;
            var success = int.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to int. If fails return the default value.
        /// </summary>
        public static int ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, int defaultValue)
        {
            int result;
            var success = int.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to int. If fails return the default value.
        /// </summary>
        public static int ParseOrDefault(ReadOnlySpan<char> target, int defaultValue)
        {
            int result;
            var success = int.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to int. If fails return the default value.
        /// </summary>
        public static int ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, int defaultValue)
        {
            int result;
            var success = int.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default long

        /// <summary>
        /// Tries to convert the target string to Int64. If fails return the default value.
        /// </summary>
        public static long ParseOrDefault(string target, long defaultValue)
        {
            long result;
            var success = long.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Int64. If fails return the default value.
        /// </summary>
        public static long ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, long defaultValue)
        {
            long result;
            var success = long.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to Int64. If fails return the default value.
        /// </summary>
        public static long ParseOrDefault(ReadOnlySpan<char> target, long defaultValue)
        {
            long result;
            var success = long.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Int64. If fails return the default value.
        /// </summary>
        public static long ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, long defaultValue)
        {
            long result;
            var success = long.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default sbyte

        /// <summary>
        /// Tries to convert the target string to SByte. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static sbyte ParseOrDefault(string target, sbyte defaultValue)
        {
            sbyte result;
            var success = sbyte.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to SByte. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static sbyte ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, sbyte defaultValue)
        {
            sbyte result;
            var success = sbyte.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to SByte. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static sbyte ParseOrDefault(ReadOnlySpan<char> target, sbyte defaultValue)
        {
            sbyte result;
            var success = sbyte.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to SByte. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static sbyte ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, sbyte defaultValue)
        {
            sbyte result;
            var success = sbyte.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default float

        /// <summary>
        /// Tries to convert the target string to Single. If fails return the default value.
        /// </summary>
        public static float ParseOrDefault(string target, float defaultValue)
        {
            float result;
            var success = float.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Single. If fails return the default value.
        /// </summary>
        public static double ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, float defaultValue)
        {
            float result;
            var success = float.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to Single. If fails return the default value.
        /// </summary>
        public static float ParseOrDefault(ReadOnlySpan<char> target, float defaultValue)
        {
            float result;
            var success = float.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Single. If fails return the default value.
        /// </summary>
        public static double ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, float defaultValue)
        {
            float result;
            var success = float.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default ushort

        /// <summary>
        /// Tries to convert the target string to UInt16. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static ushort ParseOrDefault(string target, ushort defaultValue)
        {
            ushort result;
            var success = ushort.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to UInt16. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static ushort ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, ushort defaultValue)
        {
            ushort result;
            var success = ushort.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to UInt16. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static ushort ParseOrDefault(ReadOnlySpan<char> target, ushort defaultValue)
        {
            ushort result;
            var success = ushort.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to UInt16. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static ushort ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, ushort defaultValue)
        {
            ushort result;
            var success = ushort.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default uint

        /// <summary>
        /// Tries to convert the target string to Uint. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static uint ParseOrDefault(string target, uint defaultValue)
        {
            uint result;
            var success = uint.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Uint. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static uint ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, uint defaultValue)
        {
            uint result;
            var success = uint.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to Uint. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static uint ParseOrDefault(ReadOnlySpan<char> target, uint defaultValue)
        {
            uint result;
            var success = uint.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Uint. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static uint ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, uint defaultValue)
        {
            uint result;
            var success = uint.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default ulong

        /// <summary>
        /// Tries to convert the target string to UInt64. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static ulong ParseOrDefault(string target, ulong defaultValue)
        {
            ulong result;
            var success = ulong.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to UInt64. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static ulong ParseOrDefault(string target, NumberStyles style, IFormatProvider provider, ulong defaultValue)
        {
            ulong result;
            var success = ulong.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to UInt64. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static ulong ParseOrDefault(ReadOnlySpan<char> target, ulong defaultValue)
        {
            ulong result;
            var success = ulong.TryParse(target, out result);
            return success ? result : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to UInt64. If fails return the default value.
        /// </summary>
        [CLSCompliant(false)]
        public static ulong ParseOrDefault(ReadOnlySpan<char> target, NumberStyles style, IFormatProvider provider, ulong defaultValue)
        {
            ulong result;
            var success = ulong.TryParse(target, style, provider, out result);
            return success ? result : defaultValue;
        }
#endif

        #endregion

        #region Parse or default enum

        /// <summary>
        /// Tries to convert the target string to Enum. If fails return the default value.
        /// </summary>
        public static T ParseOrDefault<T>(string target, T defaultValue) where T : Enum
        {
            return Enum.IsDefined(typeof(T), target) ? (T)Enum.Parse(typeof(T), target, true) : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Enum. If fails return the default value.
        /// </summary>
        public static T ParseOrDefault<T>(string target, bool ignoreCase, T defaultValue) where T : Enum
        {
            return Enum.IsDefined(typeof(T), target) ? (T)Enum.Parse(typeof(T), target, ignoreCase) : defaultValue;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Tries to convert the target string to Enum. If fails return the default value.
        /// </summary>
        public static T ParseOrDefault<T>(ReadOnlySpan<char> target, T defaultValue) where T : Enum
        {
            return Enum.IsDefined(typeof(T), target.ToString()) ? (T)Enum.Parse(typeof(T), target, true) : defaultValue;
        }

        /// <summary>
        /// Tries to convert the target string to Enum. If fails return the default value.
        /// </summary>
        public static T ParseOrDefault<T>(ReadOnlySpan<char> target, bool ignoreCase, T defaultValue) where T : Enum
        {
            return Enum.IsDefined(typeof(T), target.ToString()) ? (T)Enum.Parse(typeof(T), target, ignoreCase) : defaultValue;
        }
#endif

        #endregion
    }
}
