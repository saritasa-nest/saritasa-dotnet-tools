// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace Saritasa.Tools.EF.ObjectContext
{
    /// <summary>
    /// Extensions for <see cref="System.Data.Entity.Core.Objects.ObjectContext" />.
    /// </summary>
    public static class ObjectContextExtensions
    {
        /// <summary>
        /// Set for every <see cref="System.Data.Entity.Core.Objects.ObjectSet{TEntity}" /> in context specific
        /// <see cref="System.Data.Entity.Core.Objects.MergeOption" /> option.
        /// </summary>
        /// <param name="target">Context to be modified.</param>
        /// <param name="mergeOption">Merge option setup.</param>
        public static void SetMergeOption(this System.Data.Entity.Core.Objects.ObjectContext target, MergeOption mergeOption)
        {
            var properties = TypeDescriptor
                .GetProperties(target.GetType())
                .OfType<PropertyDescriptor>()
                .Where(x => typeof(ObjectQuery).IsAssignableFrom(x.PropertyType));

            foreach (var property in properties)
            {
                var set = property.GetValue(target) as ObjectQuery;
                if (set != null)
                {
                    set.MergeOption = mergeOption;
                }
            }
        }
    }
}
