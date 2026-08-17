using Saritasa.Tools.CodeAnalyzers.Helpers;
using Xunit;

namespace Saritasa.Tools.CodeAnalyzers.Tests.Helpers.StringHelperTests;

/// <summary>
/// Tests for <see cref="StringHelper.SplitByNonLetters"/>.
/// </summary>
public class SplitByNonLettersTests
{
    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> splits text with single word correctly.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_SingleWord_ReturnsWordWithOffset()
    {
        // Arrange
        const string text = "hello";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(1, result.Count);

        Assert.Equal("hello", result[0].Word);
        Assert.Equal(0, result[0].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> splits text with multiple words separated by spaces.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_MultipleWordsSeparatedBySpaces_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "hello world";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("hello", result[0].Word);
        Assert.Equal(0, result[0].Offset);

        Assert.Equal("world", result[1].Word);
        Assert.Equal(6, result[1].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> splits text with punctuation.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_WordsWithPunctuation_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "hello, world!";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("hello", result[0].Word);
        Assert.Equal(0, result[0].Offset);

        Assert.Equal("world", result[1].Word);
        Assert.Equal(7, result[1].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> handles empty string.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_EmptyString_ReturnsEmptyCollection()
    {
        // Arrange
        const string text = "";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(0, result.Count);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> handles string with only non-letters.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_OnlyNonLetters_ReturnsEmptyCollection()
    {
        // Arrange
        const string text = "123!@#";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(0, result.Count);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> handles string starting with non-letters.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_StartsWithNonLetters_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "  hello";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(1, result.Count);

        Assert.Equal("hello", result[0].Word);
        Assert.Equal(2, result[0].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> handles string ending with non-letters.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_EndsWithNonLetters_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "hello  ";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(1, result.Count);

        Assert.Equal("hello", result[0].Word);
        Assert.Equal(0, result[0].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> handles multiple consecutive non-letters.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_MultipleConsecutiveNonLetters_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "hello!!!world";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("hello", result[0].Word);
        Assert.Equal(0, result[0].Offset);

        Assert.Equal("world", result[1].Word);
        Assert.Equal(8, result[1].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> handles mixed letters and numbers.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_MixedLettersAndNumbers_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "hello123world";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("hello", result[0].Word);
        Assert.Equal(0, result[0].Offset);

        Assert.Equal("world", result[1].Word);
        Assert.Equal(8, result[1].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> handles single letter words.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_SingleLetterWords_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "a b c";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Equal("a", result[0].Word);
        Assert.Equal(0, result[0].Offset);

        Assert.Equal("b", result[1].Word);
        Assert.Equal(2, result[1].Offset);

        Assert.Equal("c", result[2].Word);
        Assert.Equal(4, result[2].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> handles complex text with various separators.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_ComplexText_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "Hello, world! This is a test.";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(6, result.Count);

        Assert.Equal("Hello", result[0].Word);
        Assert.Equal(0, result[0].Offset);

        Assert.Equal("world", result[1].Word);
        Assert.Equal(7, result[1].Offset);

        Assert.Equal("This", result[2].Word);
        Assert.Equal(14, result[2].Offset);

        Assert.Equal("is", result[3].Word);
        Assert.Equal(19, result[3].Offset);

        Assert.Equal("a", result[4].Word);
        Assert.Equal(22, result[4].Offset);

        Assert.Equal("test", result[5].Word);
        Assert.Equal(24, result[5].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> treats apostrophe in the middle as whole word.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_WordsWithApostropheInTheMiddle_ReturnsWords()
    {
        // Arrange
        const string text = "children's employee's";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("children's", result[0].Word);
        Assert.Equal(0, result[0].Offset);

        Assert.Equal("employee's", result[1].Word);
        Assert.Equal(11, result[1].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> handles words with apostrophe in the end.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_WordsWithApostropheInTheEnd_ReturnsWordsWithoutApostrophe()
    {
        // Arrange
        const string text = "parents' employees'";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("parents", result[0].Word);
        Assert.Equal(0, result[0].Offset);

        Assert.Equal("employees", result[1].Word);
        Assert.Equal(9, result[1].Offset);
    }

    /// <summary>
    /// Test that <see cref="StringHelper.SplitByNonLetters"/> handles words with apostrophe at the start.
    /// </summary>
    [Fact]
    public void SplitByNonLetters_WordsWithApostropheAtTheStart_ReturnsWordsWithoutApostrophe()
    {
        // Arrange
        const string text = "'hello' 'world'";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("hello", result[0].Word);
        Assert.Equal(1, result[0].Offset);

        Assert.Equal("world", result[1].Word);
        Assert.Equal(9, result[1].Offset);
    }
}
