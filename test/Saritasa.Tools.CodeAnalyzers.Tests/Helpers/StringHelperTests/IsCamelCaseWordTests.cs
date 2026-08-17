using Saritasa.Tools.CodeAnalyzers.Helpers;
using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.Helpers.StringHelperTests;

/// <summary>
/// Tests for <see cref="StringHelper.IsCamelCaseWord"/>.
/// </summary>
public class IsCamelCaseWordTests
{
    /// <summary>
    /// Test that camelCase words are correctly identified.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_CamelCaseWord_ReturnsTrue()
    {
        // Arrange
        const string word = "camelCase";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.True(isCamelCase);
    }

    /// <summary>
    /// Test that PascalCase words are correctly identified.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_PascalCaseWord_ReturnsTrue()
    {
        // Arrange
        const string word = "PascalCase";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.True(isCamelCase);
    }

    /// <summary>
    /// Test that all lowercase words are not identified as camelCase.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_AllLowercase_ReturnsFalse()
    {
        // Arrange
        const string word = "lowercase";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.False(isCamelCase);
    }

    /// <summary>
    /// Test that all uppercase words are not identified as camelCase.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_AllUppercase_ReturnsFalse()
    {
        // Arrange
        const string word = "UPPERCASE";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.False(isCamelCase);
    }

    /// <summary>
    /// Test that words with numbers are not identified as camelCase.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_WordWithNumbers_ReturnsFalse()
    {
        // Arrange
        const string word = "camel123Case";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.False(isCamelCase);
    }

    /// <summary>
    /// Test that words with special characters are not identified as camelCase.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_WordWithSpecialCharacters_ReturnsFalse()
    {
        // Arrange
        const string word = "camel_Case";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.False(isCamelCase);
    }

    /// <summary>
    /// Test that single letter words are not identified as camelCase.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_SingleLetter_ReturnsFalse()
    {
        // Arrange
        const string word = "a";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.False(isCamelCase);
    }

    /// <summary>
    /// Test that empty strings are not identified as camelCase.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_EmptyString_ReturnsFalse()
    {
        // Arrange
        var word = string.Empty;

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.False(isCamelCase);
    }

    /// <summary>
    /// Test that words with multiple uppercase letters are correctly identified as camelCase.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_MultipleUppercase_ReturnsTrue()
    {
        // Arrange
        const string word = "HTMLParser";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.True(isCamelCase);
    }

    /// <summary>
    /// Test that two-letter camelCase words are correctly identified.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_TwoLetterCamelCase_ReturnsTrue()
    {
        // Arrange
        const string word = "aB";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.True(isCamelCase);
    }

    /// <summary>
    /// Test that two-letter PascalCase words are correctly identified.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_TwoLetterPascalCase_ReturnsTrue()
    {
        // Arrange
        const string word = "Ab";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.True(isCamelCase);
    }

    /// <summary>
    /// Test that words with spaces are not identified as camelCase.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_WordWithSpaces_ReturnsFalse()
    {
        // Arrange
        const string word = "camel Case";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.False(isCamelCase);
    }

    /// <summary>
    /// Test that words with hyphens are not identified as camelCase.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_WordWithHyphens_ReturnsFalse()
    {
        // Arrange
        const string word = "camel-Case";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.False(isCamelCase);
    }

    /// <summary>
    /// Test that complex camelCase identifiers are correctly identified.
    /// </summary>
    [Fact]
    public void IsCamelCaseWord_ComplexCamelCase_ReturnsTrue()
    {
        // Arrange
        const string word = "thisIsAVeryLongVariableName";

        // Act
        var isCamelCase = StringHelper.IsCamelCaseWord(word);

        // Assert
        Assert.True(isCamelCase);
    }
}
