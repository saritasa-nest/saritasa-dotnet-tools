// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands
{
    using System;
    using System.Collections.Generic;
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
    using System.ComponentModel.DataAnnotations;
#endif
    using Exceptions;

    /// <summary>
    /// Wraps set of object validation exceptions. Domain exception.
    /// </summary>
    public class CommandValidationException : DomainException
    {
        /// <summary>
        /// Broken requirement.
        /// </summary>
        public struct BrokenRule
        {
            /// <summary>
            /// Class property name.
            /// </summary>
            public string PropertyName { get; private set; }

            /// <summary>
            /// Validation exception message.
            /// </summary>
            public string Message { get; private set; }

            /// <summary>
            /// .ctor
            /// </summary>
            /// <param name="propertyName">Property name.</param>
            /// <param name="message">Validation message.</param>
            public BrokenRule(string propertyName, string message)
            {
                this.PropertyName = propertyName;
                this.Message = message;
            }
        }

        readonly List<BrokenRule> brokenRules = new List<BrokenRule>();

        /// <summary>
        /// List of broken rules.
        /// </summary>
        public IEnumerable<BrokenRule> BrokenRules => brokenRules;

        /// <summary>
        /// Has broken rules.
        /// </summary>
        public bool HasErrors => brokenRules.Count > 0;

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="validationResults">Validation results.</param>
        public CommandValidationException(IList<ValidationResult> validationResults)
        {
            if (validationResults == null)
            {
                throw new ArgumentNullException(nameof(validationResults));
            }

            foreach (var validationResult in validationResults)
            {
                foreach (var member in validationResult.MemberNames)
                {
                    brokenRules.Add(new BrokenRule(member, validationResult.ErrorMessage));
                }
            }
        }
#endif

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="brokenRules">Validation results.</param>
        public CommandValidationException(IEnumerable<BrokenRule> brokenRules)
        {
            if (brokenRules == null)
            {
                throw new ArgumentNullException(nameof(brokenRules));
            }
            this.brokenRules.AddRange(brokenRules);
        }
    }
}
