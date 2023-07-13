// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Saritasa.Tools.Emails.Interceptors;

/// <summary>
/// The interceptor contains list of regular expressions to filter emails.
/// The filter method should be overridden.
/// </summary>
public abstract class EmailPatternsInterceptor : DelegateFilterInterceptor
{
    private readonly IList<Regex> patterns = new List<Regex>();

    /// <summary>
    /// Get patterns.
    /// </summary>
    public IEnumerable<Regex> Patterns => patterns;

    /// <summary>
    /// Adds regular expression pattern.
    /// </summary>
    /// <param name="regex">Regular expression.</param>
    public void AddPattern(Regex regex)
    {
        if (regex == null)
        {
            throw new ArgumentNullException(nameof(regex));
        }
        if (patterns.Contains(regex))
        {
            return;
        }

        patterns.Add(regex);
    }

    /// <summary>
    /// Adds patterns. You can use ? and * symbols. If several patterns are specified use comma to separate them.
    /// </summary>
    public void AddPattern(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        foreach (var patternItem in SplitAndCleanEmails(pattern))
        {
            var regex = new Regex(WildcardToRegex(patternItem),
                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            AddPattern(regex);
        }
    }

    /// <summary>
    /// Clears patterns list.
    /// </summary>
    public void Clear()
    {
        patterns.Clear();
    }

    /// <summary>
    /// Convert string with emails to array.
    /// </summary>
    /// <param name="emails">String with emails, for example "ivan@saritasa.com,test@example.com".</param>
    /// <returns>Array of emails.</returns>
    internal static string[] SplitAndCleanEmails(string emails) => emails
        .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(e => e.ToLowerInvariant().Trim())
        .Where(e => !string.IsNullOrEmpty(e))
        .ToArray();

    /// <summary>
    /// Converts wildcards to regex. Determines what reg exp correspond to string with * and ? chars.
    /// </summary>
    /// <param name="pattern">The wildcards pattern.</param>
    /// <returns>Pattern string.</returns>
    internal static string WildcardToRegex(string pattern) =>
        ("^" + Regex.Escape(pattern)).Replace("\\*", ".*").Replace("\\?", ".") + "$";
}
