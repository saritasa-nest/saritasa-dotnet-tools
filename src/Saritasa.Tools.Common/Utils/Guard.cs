// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Saritasa.Tools.Common.Extensions;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Contains various check methods. If condition is false it generates exception.
    /// </summary>
    public static class Guard
    {
#if NETSTANDARD1_2
        internal const RegexOptions Options = RegexOptions.Singleline;
#else
        internal const RegexOptions Options = RegexOptions.Singleline | RegexOptions.Compiled;
#endif

        /// <summary>
        /// Email check regular expression.
        /// </summary>
        public static readonly Regex EmailExpression = new Regex(@"^([0-9a-zA-Z]+[-._+&])*[0-9a-zA-Z]+@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$", Options);

        /// <summary>
        /// Web url check regular expression.
        /// </summary>
        public static readonly Regex WebUrlExpression = new Regex(@"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", Options);

        /// <summary>
        /// Regular expression to strip html tags.
        /// </summary>
        public static readonly Regex StripHtmlExpression = new Regex("<\\S[^><]*>", Options | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);

        /// <summary>
        /// Is not empty check for guid. Generates <see cref="ArgumentException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotEmpty(Guid argument, string argumentName)
        {
            if (argument == Guid.Empty)
            {
                throw new ArgumentException(Properties.Strings.ArgumentCannotBeEmptyGuid.FormatWith(argumentName),
                    argumentName);
            }
        }

        /// <summary>
        /// Is not empty check for string. Generates <see cref="ArgumentException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotEmpty(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException(Properties.Strings.ArgumentCannotBeEmptyString.FormatWith(argumentName),
                    argumentName);
            }
        }

        /// <summary>
        /// Is not out of length check. Generates <see cref="ArgumentException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="length">Maximum length.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotOutOfLength(string argument, int length, string argumentName)
        {
            if (argument.Length > length)
            {
                throw new ArgumentException(
                    Properties.Strings.ArgumentCannotBeMoreThanChars.FormatWith(argumentName, length.ToString()), argumentName);
            }
        }

        /// <summary>
        /// Is not null check. Generates <see cref="ArgumentNullException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Is not negative check for integer. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNegative(int argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not negative or zero check. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNegativeOrZero(int argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not negative check for Int64. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNegative(Int64 argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not negative check for Int64. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNegativeOrZero(Int64 argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not negative check for Single. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNegative(Single argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not negative or zero check for Single. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNegativeOrZero(Single argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not negative check for Decimal. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNegative(Decimal argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not negative or zero check for Decimal. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNegativeOrZero(Decimal argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not in past check for DateTime. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotInPast(DateTime argument, string argumentName)
        {
            if (argument < DateTime.Now)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not in past check for DateTime according to specific date.
        /// Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="date">Date to compare.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotInPast(DateTime argument, DateTime date, string argumentName)
        {
            if (argument < date)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not in future check for DateTime. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotInFuture(DateTime argument, string argumentName)
        {
            if (argument > DateTime.Now)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not in future check for <see cref="DateTime" /> according to specific date.
        /// Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="date">Date to compare.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotInFuture(DateTime argument, DateTime date, string argumentName)
        {
            if (argument > date)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not negative check for TimeSpan. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNegative(TimeSpan argument, string argumentName)
        {
            if (argument < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not negative check or zero for TimeSpan. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotNegativeOrZero(TimeSpan argument, string argumentName)
        {
            if (argument <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Is not empty check for collection of arguments. Generates <see cref="ArgumentException" />.
        /// </summary>
        /// <param name="argument">Collection of arguments.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotEmpty<T>(ICollection<T> argument, string argumentName)
        {
            IsNotNull(argument, argumentName);

            if (argument.Count == 0)
            {
                throw new ArgumentException(Properties.Strings.CollectionCannotEmpty, argumentName);
            }
        }

        /// <summary>
        /// Is in range check. Generates <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <param name="argument">Collection of arguments.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotOutOfRange(int argument, int min, int max, string argumentName)
        {
            if (argument < min || argument > max)
            {
                throw new ArgumentOutOfRangeException(argumentName,
                    Properties.Strings.ArgumentMustBeBetween.FormatWith(argumentName, min.ToString(), max.ToString()));
            }
        }

        /// <summary>
        /// Is not invalid email check. Generates <see cref="ArgumentException" />.
        /// </summary>
        /// <param name="argument">Email argument.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void IsNotInvalidEmail(string argument, string argumentName)
        {
            IsNotEmpty(argument, argumentName);

            if (!EmailExpression.IsMatch(argument))
            {
                throw new ArgumentException(Properties.Strings.ArgumentNotValidEmail.FormatWith(argumentName), argumentName);
            }
        }
    }
}
