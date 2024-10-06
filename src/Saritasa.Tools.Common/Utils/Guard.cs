// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Saritasa.Tools.Common.Extensions;

namespace Saritasa.Tools.Common.Utils;

/// <summary>
/// Contains various check methods. If condition is <c>false</c> it generates exception.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Regex options.
    /// </summary>
    internal const RegexOptions Options = RegexOptions.Singleline | RegexOptions.Compiled;

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
    /// <exception cref="ArgumentException">Occurs when argument is empty.</exception>
    public static void IsNotEmpty(Guid argument, string argumentName)
    {
        if (argument == Guid.Empty)
        {
            throw new ArgumentException(string.Format(Properties.Strings.ArgumentCannotBeEmptyGuid, argumentName),
                argumentName);
        }
    }

    /// <summary>
    /// Is not empty check for string. Generates <see cref="ArgumentException" />.
    /// </summary>
    /// <param name="argument">Argument.</param>
    /// <param name="argumentName">Argument name.</param>
    /// <exception cref="ArgumentException">Occurs when argument is empty.</exception>
    public static void IsNotEmpty(string argument, string argumentName)
    {
        if (string.IsNullOrEmpty(argument))
        {
            throw new ArgumentException(string.Format(Properties.Strings.ArgumentCannotBeEmptyString, argumentName),
                argumentName);
        }
    }

    /// <summary>
    /// Is not empty or white space check for string. Generates <see cref="ArgumentException" />.
    /// </summary>
    /// <param name="argument">Argument.</param>
    /// <param name="argumentName">Argument name.</param>
    /// <exception cref="ArgumentException">Occurs when argument is empty or white space.</exception>
    public static void IsNotEmptyOrWhiteSpace(string argument, string argumentName)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException(string.Format(Properties.Strings.ArgumentCannotBeEmptyString, argumentName),
                argumentName);
        }
    }

    /// <summary>
    /// Is not out of length check. Generates <see cref="ArgumentException" />.
    /// </summary>
    /// <param name="argument">Argument.</param>
    /// <param name="length">Maximum length.</param>
    /// <param name="argumentName">Argument name.</param>
    /// <exception cref="ArgumentException">Occurs when argument is out of length.</exception>
    public static void IsNotOutOfLength(string argument, int length, string argumentName)
    {
        if (argument.Length > length)
        {
            throw new ArgumentException(
                string.Format(Properties.Strings.ArgumentCannotBeMoreThanChars, argumentName, length.ToString()),
                argumentName);
        }
    }

    /// <summary>
    /// Is not null check. Generates <see cref="ArgumentNullException" />.
    /// </summary>
    /// <param name="argument">Argument.</param>
    /// <param name="argumentName">Argument name.</param>
    /// <exception cref="ArgumentNullException">Occurs when argument is null.</exception>
    public static void IsNotNull(object? argument, string argumentName)
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is out of range.</exception>
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is out of range.</exception>
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is negative.</exception>
    public static void IsNotNegative(long argument, string argumentName)
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is negative or zero.</exception>
    public static void IsNotNegativeOrZero(long argument, string argumentName)
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is negative.</exception>
    public static void IsNotNegative(float argument, string argumentName)
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is negative or zero.</exception>
    public static void IsNotNegativeOrZero(float argument, string argumentName)
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is negative.</exception>
    public static void IsNotNegative(decimal argument, string argumentName)
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is not negative or zero.</exception>
    public static void IsNotNegativeOrZero(decimal argument, string argumentName)
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is in past.</exception>
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is in past.</exception>
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is in future.</exception>
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is in future.</exception>
    public static void IsNotInFuture(DateTime argument, DateTime date, string argumentName)
    {
        if (argument > date)
        {
            throw new ArgumentOutOfRangeException(argumentName);
        }
    }

    /// <summary>
    /// Is not negative check for <see cref="TimeSpan" />. Generates <see cref="ArgumentOutOfRangeException" />.
    /// </summary>
    /// <param name="argument">Argument.</param>
    /// <param name="argumentName">Argument name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is negative.</exception>
    public static void IsNotNegative(TimeSpan argument, string argumentName)
    {
        if (argument < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(argumentName);
        }
    }

    /// <summary>
    /// Is not negative check or zero for <see cref="TimeSpan" />. Generates <see cref="ArgumentOutOfRangeException" />.
    /// </summary>
    /// <param name="argument">Argument.</param>
    /// <param name="argumentName">Argument name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is negative or zero.</exception>
    public static void IsNotNegativeOrZero(TimeSpan argument, string argumentName)
    {
        if (argument <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(argumentName);
        }
    }

    /// <summary>
    /// Is not an empty check for the collection of arguments. Generates <see cref="ArgumentException" />.
    /// </summary>
    /// <param name="argument">Collection of arguments.</param>
    /// <param name="argumentName">Argument name.</param>
    /// <exception cref="ArgumentException">Occurs when collection is empty.</exception>
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
    /// <exception cref="ArgumentOutOfRangeException">Occurs when argument is out of range.</exception>
    public static void IsNotOutOfRange(int argument, int min, int max, string argumentName)
    {
        if (argument < min || argument > max)
        {
            throw new ArgumentOutOfRangeException(argumentName,
                string.Format(Properties.Strings.ArgumentMustBeBetween, argumentName, min.ToString(), max.ToString()));
        }
    }

    /// <summary>
    /// Is not invalid email check. Generates <see cref="ArgumentException" />.
    /// </summary>
    /// <param name="argument">Email argument.</param>
    /// <param name="argumentName">Argument name.</param>
    /// <exception cref="ArgumentException">Occurs when email is invalid.</exception>
    /// <exception cref="ArgumentException">Occurs when argument is empty.</exception>
    public static void IsNotInvalidEmail(string argument, string argumentName)
    {
        IsNotEmpty(argument, argumentName);

        if (!EmailExpression.IsMatch(argument))
        {
            throw new ArgumentException(string.Format(Properties.Strings.ArgumentNotValidEmail, argumentName),
                argumentName);
        }
    }
}
