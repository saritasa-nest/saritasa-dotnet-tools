// Copyright (c) 2015-2022, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Saritasa.Tools.Common.Utils;
using Xunit;

namespace Saritasa.Tools.Common.Tests;

/// <summary>
/// Enum tests.
/// </summary>
public class EnumTests
{
    private const string OverriddenDescriptionName = "Description Override";

    private class TestAttribute : Attribute
    {
    }

    public enum TestEnum
    {
        [Test]
        [Description(OverriddenDescriptionName)]
        A,

        B,

        Simple,

        TargetDbConnection,
    }

    [Fact]
    public void GetAttribute_EnumWithWithTestAttribute_ReturnsTestAttribute()
    {
        // Arrange
        var val = TestEnum.A;

        // Act
        var attr = EnumUtils.GetAttribute<TestAttribute>(val);

        // Assert
        Assert.NotNull(attr);
        Assert.IsType<TestAttribute>(attr);
    }

    [Fact]
    public void GetAttribute_EnumWithWithNoTestAttribute_ReturnsNull()
    {
        // Arrange
        var val = TestEnum.B;

        // Act
        var attr = EnumUtils.GetAttribute<TestAttribute>(val);

        // Assert
        Assert.Null(attr);
    }

    [Fact]
    public void GetAttribute_EnumWithWithInvalidAttribute_ReturnsNull()
    {
        // Arrange
        var val = TestEnum.A;

        // Act
        var attr = EnumUtils.GetAttribute<ObsoleteAttribute>(val);

        // Assert
        Assert.Null(attr);
    }

    [Fact]
    public void GetDescription_EnumValue_ValidSimpleString()
    {
        // Arrange
        var val = TestEnum.Simple;

        // Act
        var stringRepresentation = EnumUtils.GetDescription(val);

        // Assert
        Assert.Equal("Simple", stringRepresentation);
    }

    [Fact]
    public void GetDescription_EnumValue_ValidSmartString()
    {
        // Arrange
        var val = TestEnum.TargetDbConnection;

        // Act
        var stringRepresentation = EnumUtils.GetDescription(val);

        // Assert
        Assert.Equal("Target Db Connection", stringRepresentation);
    }

    [Fact]
    public void GetDescription_EnumValue_ValidStringFromDescriptionAttribute()
    {
        // Arrange
        var val = TestEnum.A;

        // Act
        var stringRepresentation = EnumUtils.GetDescription(val);

        // Assert
        Assert.Equal(OverriddenDescriptionName, stringRepresentation);
    }

    [Fact]
    public void GetNamesWithDescriptions_Enum_KeyValuePairsOfNamesAndDescriptions()
    {
        // Act
        var enumNamesDescriptions = EnumUtils.GetNamesWithDescriptions<TestEnum>().ToArray();

        // Assert
        Assert.Equal(4, enumNamesDescriptions.Length);
        Assert.Equal(new KeyValuePair<string, string>("B", "B"), enumNamesDescriptions[1]);
    }

    [Fact]
    public void GetValuesWithDescriptions_Enum_KeyValuePairsOfValuesAndDescriptions()
    {
        // Act
        var enumNamesDescriptions = EnumUtils.GetValuesWithDescriptions<TestEnum>().ToArray();

        // Assert
        Assert.Equal(4, enumNamesDescriptions.Length);
        Assert.Equal(new KeyValuePair<string, string>("1", "B"), enumNamesDescriptions[1]);
    }
}
