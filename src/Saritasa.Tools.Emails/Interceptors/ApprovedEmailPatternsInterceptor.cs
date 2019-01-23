// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Emails.Interceptors
{
    /// <summary>
    /// The interceptor specifies patterns to whom emails can be sent. If email does not match
    /// any pattern it will be filtered.
    /// </summary>
    public class ApprovedEmailPatternsInterceptor : EmailPatternsInterceptor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ApprovedEmailPatternsInterceptor()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="patterns">Approved emails patterns. You can use ? and * symbols.</param>
        public ApprovedEmailPatternsInterceptor(string patterns) : this()
        {
            if (string.IsNullOrEmpty(patterns))
            {
                throw new ArgumentNullException(nameof(patterns));
            }
            AddPattern(patterns);
        }

        /// <summary>
        /// If email does not match any pattern it will be filtered.
        /// </summary>
        /// <param name="email">Email to filter.</param>
        /// <returns><c>True</c> if email should be filtered or <c>false</c> otherwise.</returns>
        protected override bool ShouldEmailBeFiltered(string email)
        {
            foreach (var pattern in Patterns)
            {
                if (pattern.IsMatch(email))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
