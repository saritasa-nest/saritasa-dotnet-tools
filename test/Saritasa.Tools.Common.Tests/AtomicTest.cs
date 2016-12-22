// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Tests
{
    using Xunit;
    using Utils;

    /// <summary>
    /// Atomic tests.
    /// </summary>
    public class AtomicTest
    {
        [Fact]
        public void Cas_should_process()
        {
            int a = 5;
            AtomicUtils.DoWithCas(ref a, v => v * 15);
            Assert.Equal(75, a);
        }

        [Fact]
        public void Atomic_swap_should_swap_values()
        {
            int a = 2, b = 5;
            AtomicUtils.Swap(ref a, ref b);
            Assert.Equal(5, a);
            Assert.Equal(2, b);
        }
    }
}
