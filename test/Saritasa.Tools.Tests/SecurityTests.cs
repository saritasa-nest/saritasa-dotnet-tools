// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Xunit;
using Saritasa.Tools.Misc.Security;
#pragma warning disable CS1591

namespace Saritasa.Tools.Tests;

/// <summary>
/// Security tests.
/// </summary>
public class SecurityTests
{
    [Fact]
    public void Generate_DigitsShuffleChars25Length_ExpectedPassword()
    {
        // Arrange
        var passwordGenerator = new PasswordGenerator(
            25,
            PasswordGenerator.CharacterClass.Digits,
            PasswordGenerator.GeneratorFlag.ShuffleChars
        );

        // Act
        var password = passwordGenerator.Generate();

        // Assert
        Assert.Equal(25, password.Length);
    }

    [Fact]
    public void Generate_CustomCharactersPool15Length_ExpectedPassword()
    {
        // Arrange
        var passwordGenerator = new PasswordGenerator();
        passwordGenerator.PasswordLength = 15;
        passwordGenerator.SetCharactersPool("1");

        // Act
        var password = passwordGenerator.Generate();

        // Assert
        Assert.Equal(new string('1', 15), password);
    }

    [Fact]
    public void GetEntropy_10LengthNumbersPassword_EntropyInRange()
    {
        // Arrange
        PasswordGenerator passwordGenerator = new PasswordGenerator();
        passwordGenerator.PasswordLength = 10;
        passwordGenerator.SetCharactersPool("0123456789"); // 10 symbols

        // Act
        var entropy = passwordGenerator.GetEntropy();

        // Assert
        Assert.InRange(entropy, 33.119, 33.319);
    }

    [Theory]
    [InlineData(0, "1111")]
    [InlineData(0, "2222222222")]
    [InlineData(4, "123456789")]
    [InlineData(73, "123456789A")]
    [InlineData(75, "123456789AB")]
    [InlineData(0, "CCCCCCCCC")]
    [InlineData(100, "8m6y2L2WhalkPDa")]
    [InlineData(68, "AA11bb00__")]
    [InlineData(89, "123456_789AB")]
    public void EstimatePasswordStrength_DifferentPasswords_Matches(int expectedStrength, string password)
    {
        // Arrange, act & assert
        Assert.Equal(expectedStrength, PasswordGenerator.EstimatePasswordStrength(password));
    }
}
