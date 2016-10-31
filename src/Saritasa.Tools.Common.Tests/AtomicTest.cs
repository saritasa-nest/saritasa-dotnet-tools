// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Tests
{
    using System;
    using NUnit.Framework;
    using Extensions;
    using Utils;

    /// <summary>
    /// Atomic tests.
    /// </summary>
    [TestFixture]
    public class AtomicTest
    {
        [Test]
        public void Cas_should_process()
        {
            int a = 5;
            AtomicUtils.DoWithCas(ref a, v => v * 15);
            Assert.That(a, Is.EqualTo(75));
        }

        [Test]
        public void Atomic_swap_should_swap_values()
        {
            int a = 2, b = 5;
            AtomicUtils.Swap(ref a, ref b);
            Assert.That(a, Is.EqualTo(5));
            Assert.That(b, Is.EqualTo(2));
        }
    }
}
