using Microsoft.VisualStudio.TestTools.UnitTesting;
using Saritasa.Tools.CodeAnalyzers.Helpers;

namespace Saritasa.Tools.CodeAnalyzers.Tests.Helpers.StringHelperTests;

/// <summary>
/// Tests for <see cref="StringHelper.SplitCamelCase"/>.
/// </summary>
[TestClass]
public class SplitCamelCaseTests
{
    /// <summary>
    /// Test that SplitCamelCase splits a simple camelCase word correctly.
    /// </summary>
    [TestMethod]
    public void SplitCamelCase_SimpleCamelCase_ReturnsPartsWithOffsets()
    {
        // Arrange
        const string word = "camelCase";
        const int baseOffset = 0;

        // Act
        var result = StringHelper.SplitCamelCase(word, baseOffset).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("camel", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("Case", result[1].Word);
        Assert.AreEqual(5, result[1].Offset);
    }

    /// <summary>
    /// Test that SplitCamelCase splits a PascalCase word correctly.
    /// </summary>
    [TestMethod]
    public void SplitCamelCase_PascalCase_ReturnsPartsWithOffsets()
    {
        // Arrange
        const string word = "PascalCase";
        const int baseOffset = 0;

        // Act
        var result = StringHelper.SplitCamelCase(word, baseOffset).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("Pascal", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("Case", result[1].Word);
        Assert.AreEqual(6, result[1].Offset);
    }

    /// <summary>
    /// Test that SplitCamelCase handles acronyms correctly.
    /// </summary>
    [TestMethod]
    public void SplitCamelCase_WithAcronym_ReturnsPartsWithOffsets()
    {
        // Arrange
        const string word = "XMLParser";
        const int baseOffset = 0;

        // Act
        var result = StringHelper.SplitCamelCase(word, baseOffset).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("XML", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("Parser", result[1].Word);
        Assert.AreEqual(3, result[1].Offset);
    }

    /// <summary>
    /// Test that SplitCamelCase handles multiple parts correctly.
    /// </summary>
    [TestMethod]
    public void SplitCamelCase_MultipleParts_ReturnsPartsWithOffsets()
    {
        // Arrange
        const string word = "camelCaseWord";
        const int baseOffset = 0;

        // Act
        var result = StringHelper.SplitCamelCase(word, baseOffset).ToList();

        // Assert
        Assert.AreEqual(3, result.Count);

        Assert.AreEqual("camel", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("Case", result[1].Word);
        Assert.AreEqual(5, result[1].Offset);

        Assert.AreEqual("Word", result[2].Word);
        Assert.AreEqual(9, result[2].Offset);
    }

    /// <summary>
    /// Test that SplitCamelCase handles single word correctly.
    /// </summary>
    [TestMethod]
    public void SplitCamelCase_SingleWord_ReturnsSinglePart()
    {
        // Arrange
        const string word = "word";
        const int baseOffset = 0;

        // Act
        var result = StringHelper.SplitCamelCase(word, baseOffset).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);

        Assert.AreEqual("word", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);
    }

    /// <summary>
    /// Test that SplitCamelCase applies base offset correctly.
    /// </summary>
    [TestMethod]
    public void SplitCamelCase_WithBaseOffset_ReturnsCorrectOffsets()
    {
        // Arrange
        const string word = "camelCase";
        const int baseOffset = 10;

        // Act
        var result = StringHelper.SplitCamelCase(word, baseOffset).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("camel", result[0].Word);
        Assert.AreEqual(10, result[0].Offset);

        Assert.AreEqual("Case", result[1].Word);
        Assert.AreEqual(15, result[1].Offset);
    }

    /// <summary>
    /// Test that SplitCamelCase handles empty string correctly.
    /// </summary>
    [TestMethod]
    public void SplitCamelCase_EmptyString_ReturnsEmpty()
    {
        // Arrange
        const string word = "";
        const int baseOffset = 0;

        // Act
        var result = StringHelper.SplitCamelCase(word, baseOffset).ToList();

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    /// Test that SplitCamelCase handles word with non-letters correctly.
    /// </summary>
    [TestMethod]
    public void SplitCamelCase_WithNonLetters_ReturnsParts()
    {
        // Arrange
        const string word = "camelCase123";
        const int baseOffset = 0;

        // Act
        var result = StringHelper.SplitCamelCase(word, baseOffset).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual("camel", result[0].Word);
        Assert.AreEqual(0, result[0].Offset);

        Assert.AreEqual("Case", result[1].Word);
        Assert.AreEqual(5, result[1].Offset);
    }
}
