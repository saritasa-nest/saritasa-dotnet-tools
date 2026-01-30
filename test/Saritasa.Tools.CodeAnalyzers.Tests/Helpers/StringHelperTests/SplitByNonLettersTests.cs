using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Helpers;

namespace Saritasa.Tools.CodeAnalyzers.Tests.Helpers.StringHelperTests;

/// <summary>
/// Tests for <see cref="StringHelper.SplitByNonLetters"/>.
/// </summary>
[TestClass]
public class SplitByNonLettersTests
{
    /// <summary>
    /// Test that SplitByNonLetters splits text with single word correctly.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_SingleWord_ReturnsWordWithOffset()
    {
        // Arrange
        const string text = "hello";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);

        Assert.AreEqual("hello", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);
    }

    /// <summary>
    /// Test that SplitByNonLetters splits text with multiple words separated by spaces.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_MultipleWordsSeparatedBySpaces_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "hello world";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("hello", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("world", result[1].Word);
        Assert.AreEqual(6, result[1].Offset);
    }

    /// <summary>
    /// Test that SplitByNonLetters splits text with punctuation.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_WordsWithPunctuation_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "hello, world!";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("hello", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("world", result[1].Word);
        Assert.AreEqual(7, result[1].Offset);
    }

    /// <summary>
    /// Test that SplitByNonLetters handles empty string.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_EmptyString_ReturnsEmptyCollection()
    {
        // Arrange
        const string text = "";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Test that SplitByNonLetters handles string with only non-letters.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_OnlyNonLetters_ReturnsEmptyCollection()
    {
        // Arrange
        const string text = "123!@#";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Test that SplitByNonLetters handles string starting with non-letters.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_StartsWithNonLetters_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "  hello";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);

        Assert.AreEqual("hello", result[0].Word);
        Assert.AreEqual(2, result[0].Offset);
    }

    /// <summary>
    /// Test that SplitByNonLetters handles string ending with non-letters.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_EndsWithNonLetters_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "hello  ";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);

        Assert.AreEqual("hello", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);
    }

    /// <summary>
    /// Test that SplitByNonLetters handles multiple consecutive non-letters.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_MultipleConsecutiveNonLetters_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "hello!!!world";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("hello", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("world", result[1].Word);
        Assert.AreEqual(8, result[1].Offset);
    }

    /// <summary>
    /// Test that SplitByNonLetters handles mixed letters and numbers.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_MixedLettersAndNumbers_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "hello123world";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("hello", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("world", result[1].Word);
        Assert.AreEqual(8, result[1].Offset);
    }

    /// <summary>
    /// Test that SplitByNonLetters handles single letter words.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_SingleLetterWords_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "a b c";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(3, result.Count);

        Assert.AreEqual("a", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("b", result[1].Word);
        Assert.AreEqual(2, result[1].Offset);

        Assert.AreEqual("c", result[2].Word);
        Assert.AreEqual(4, result[2].Offset);
    }

    /// <summary>
    /// Test that SplitByNonLetters handles words with apostrophes.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_WordsWithApostrophes_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "don't can't";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("don", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("can", result[1].Word);
        Assert.AreEqual(6, result[1].Offset);
    }

    /// <summary>
    /// Test that SplitByNonLetters handles complex text with various separators.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_ComplexText_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "Hello, world! This is a test.";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(6, result.Count);

        Assert.AreEqual("Hello", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("world", result[1].Word);
        Assert.AreEqual(8, result[1].Offset);

        Assert.AreEqual("This", result[2].Word);
        Assert.AreEqual(16, result[2].Offset);

        Assert.AreEqual("is", result[3].Word);
        Assert.AreEqual(21, result[3].Offset);

        Assert.AreEqual("a", result[4].Word);
        Assert.AreEqual(24, result[4].Offset);

        Assert.AreEqual("test", result[5].Word);
        Assert.AreEqual(26, result[5].Offset);
    }

    /// <summary>
    /// Test that SplitByNonLetters handles Unicode letters.
    /// </summary>
    [TestMethod]
    public void SplitByNonLetters_UnicodeLetters_ReturnsWordsWithOffsets()
    {
        // Arrange
        const string text = "héllo wörld";

        // Act
        var result = StringHelper.SplitByNonLetters(text).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("héllo", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("wörld", result[1].Word);
        Assert.AreEqual(7, result[1].Offset);
    }
}

