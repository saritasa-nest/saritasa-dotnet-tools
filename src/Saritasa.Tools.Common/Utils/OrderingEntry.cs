// Copyright(c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// The class contains information about ordering by specific field.
    /// </summary>
    [DebuggerDisplay("{FieldName}: {SortDirection}")]
    public struct OrderingEntry
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fieldName">Name of the field to sort by. Ascending by default.</param>
        /// <param name="sortDirection">Sort direction.</param>
        public OrderingEntry(string fieldName, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            Saritasa.Tools.Common.Utils.Guard.IsNotNull(fieldName, nameof(fieldName));
            FieldName = fieldName;
            SortDirection = sortDirection;
        }

        /// <summary>
        /// Name of the field to sort by. Always lower case.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Sort direction.
        /// </summary>
        public ListSortDirection SortDirection { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (this.GetType() != obj.GetType())
            {
                return false;
            }

            var sortObj = (OrderingEntry)obj;
            return this.FieldName == sortObj.FieldName && this.SortDirection == sortObj.SortDirection;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return FieldName.GetHashCode() ^ SortDirection.GetHashCode();
            }
        }

        /// <inheritdoc />
        public override string ToString() => FieldName + ": " + (SortDirection == ListSortDirection.Ascending ? "asc" : "desc");
    }
}
