// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// The struct represents the pair of source item in collection and the same (by identity)
    /// item in target collection.
    /// </summary>
    /// <typeparam name="T">Source type.</typeparam>
    public struct DiffResultUpdatedItems<T>
    {
        /// <summary>
        /// Source item.
        /// </summary>
        public T Source { get; }

        /// <summary>
        /// Target item.
        /// </summary>
        public T Target { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="source">Source item.</param>
        /// <param name="target">Target item.</param>
        public DiffResultUpdatedItems(T source, T target)
        {
            this.Source = source;
            this.Target = target;
        }

        /// <summary>
        /// Deconstruction method used for tuples.
        /// </summary>
        /// <param name="source">Source item.</param>
        /// <param name="target">Target item.</param>
        public void Deconstruct(out T source, out T target)
        {
            source = this.Source;
            target = this.Target;
        }

#if NETSTANDARD1_6 || NETSTANDARD2_0
        /// <summary>
        /// Allows to implicitly convert to the type from tuple.
        /// </summary>
        /// <param name="tuple">The tuple to convert from.</param>
        /// <returns>The <see cref="DiffResultUpdatedItems{T}" /> instance.</returns>
        public static implicit operator DiffResultUpdatedItems<T>(ValueTuple<T, T> tuple)
            => new DiffResultUpdatedItems<T>(tuple.Item1, tuple.Item2);
#endif

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

            var diffObj = (DiffResultUpdatedItems<T>)obj;
            return this.Source.Equals(diffObj.Source) && this.Target.Equals(diffObj.Target);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Source.GetHashCode() ^ this.Target.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString() => this.Source + "; " + this.Target;
    }
}
