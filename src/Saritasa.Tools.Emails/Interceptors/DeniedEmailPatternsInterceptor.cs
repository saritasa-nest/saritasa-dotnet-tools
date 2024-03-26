// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Emails.Interceptors;

/// <summary>
/// The interceptor specifies patterns to whom emails should not be sent.
/// </summary>
public class DeniedEmailPatternsInterceptor : EmailPatternsInterceptor
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public DeniedEmailPatternsInterceptor()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="patterns">Denied emails patterns. You can use ? and * symbols.</param>
    public DeniedEmailPatternsInterceptor(string patterns) : this()
    {
        if (string.IsNullOrEmpty(patterns))
        {
            throw new ArgumentNullException(nameof(patterns));
        }
        AddPattern(patterns);
    }

    /// <inheritdoc />
    protected override bool ShouldEmailBeFiltered(string email)
    {
        foreach (var pattern in Patterns)
        {
            if (pattern.IsMatch(email))
            {
                return true;
            }
        }
        return false;
    }
}
