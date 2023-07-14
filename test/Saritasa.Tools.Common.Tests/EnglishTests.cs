// Copyright (c) 2015-2023, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Xunit;
using Saritasa.Tools.Common.Utils;
#pragma warning disable CS1591

namespace Saritasa.Tools.Common.Tests;

/// <summary>
/// Language tests.
/// </summary>
public class EnglishTests
{
    [Theory]
    [InlineData("dog", "dogs")]
    [InlineData("Cat", "Cats")]
    [InlineData("coffee", "coffees")]
    [InlineData("you", "you")]
    [InlineData("industry", "industries")]
    [InlineData("flush", "flushes")]
    [InlineData("half", "halves")]
    [InlineData("racehorse", "racehorses")]
    [InlineData("I", "I")]
    [InlineData("diploma", "diplomata")]
    [InlineData("alga", "algae")]
    public void English_pluralization_should_correct(string target, string expect)
    {
        Assert.Equal(expect, EnglishUtils.Pluralize(target));
    }
}
