// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Xunit;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// String helpers tests.
    /// </summary>
    public class StringHelpersTests
    {
        [Theory]
        [InlineData("item1,item2,\"item 3\",item4", new[] { "item1", "item2", "item 3", "item4" })]
        public void GetFieldsFromLine(string target, string[] expectedResult)
        {
            // Arrange & act
            var result = StringHelpers.GetFieldsFromLine(target);

            // Assert
            Assert.Equal(expectedResult.ToList(), result);
        }

        [Theory]
        [InlineData("token1 token2", new[] { ';' }, new[] { "token1", "token2" })]
        [InlineData("token1 ; t3  token2", new[] { ';' }, new[] { "token1", ";", "t3", "token2" })]
        [InlineData("a > b c=3 top 10", new[] { '<', '>', '=' }, new[] { "a", ">", "b", "c", "=", "3", "top", "10" })]
        [InlineData(" a=\"i1 i2\" and b = 'i3 i4' ", new[] { '=' }, new[] { "a", "=", "i1 i2", "and", "b", "=", "i3 i4" })]
        public void Tokenize_RawString_CorrectedArray(string target, char[] tokens, string[] expectedResult)
        {
            // Arrange & act
            var result = StringHelpers.Tokenize(target, tokens);

            // Assert
            Assert.Equal(expectedResult.ToList(), result);
        }
    }
}
