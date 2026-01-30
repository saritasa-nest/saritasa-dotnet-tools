namespace Saritasa.Tools.CodeAnalyzers.Helpers;

/// <summary>
/// String helper.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Determines whether a word is in camelCase or PascalCase format.
    /// A word is considered camelCase if it contains both uppercase and lowercase letters
    /// and consists only of letters.
    /// </summary>
    /// <param name="word">The word to check.</param>
    /// <returns><c>true</c> if the word is in camelCase/PascalCase format; otherwise, <c>false</c>.</returns>
    public static bool IsCamelCaseWord(string word)
    {
        var hasLower = false;
        var hasUpper = false;
        foreach (var character in word)
        {
            if (!char.IsLetter(character))
            {
                return false;
            }

            if (char.IsLower(character))
            {
                hasLower = true;
            }
            else if (char.IsUpper(character))
            {
                hasUpper = true;
            }

            if (hasLower && hasUpper)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Splits a camel case word into its constituent parts, returning each part with its offset relative to the base offset.
    /// </summary>
    /// <param name="word">The camel case word to split.</param>
    /// <param name="baseOffset">The base offset to add to each part's offset.</param>
    /// <returns>Enumerable of tuples containing each word part and its offset.</returns>
    /// <remarks>
    /// The offset is the start of the word index within the original string.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = StringHelper.SplitCamelCase("camelCaseWord", 0);
    /// Returns: [("camel", 0), ("Case", 5), ("Word", 9)]
    /// </code>
    /// </example>
    public static IEnumerable<(string Word, int Offset)> SplitCamelCase(string word, int baseOffset)
    {
        var start = 0;
        for (var i = 1; i < word.Length; i++)
        {
            var previous = word[i - 1];
            var current = word[i];

            var nextIsLower = i + 1 < word.Length && char.IsLower(word[i + 1]);

            if (!char.IsLetter(current))
            {
                if (i > start)
                {
                    yield return (word.Substring(start, i - start), baseOffset + start);
                }

                start = i + 1;
                continue;
            }

            var boundaryFromLowerToUpper = char.IsLower(previous) && char.IsUpper(current);
            var boundaryFromAcronymToWord = char.IsUpper(previous) && char.IsUpper(current) && nextIsLower;

            if (boundaryFromLowerToUpper || boundaryFromAcronymToWord)
            {
                yield return (word.Substring(start, i - start), baseOffset + start);
                start = i;
            }
        }

        if (start < word.Length)
        {
            yield return (word.Substring(start, word.Length - start), baseOffset + start);
        }
    }

    /// <summary>
    /// Splits a string into words by non-letter characters, returning each word with its offset.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <returns>Enumerable of tuples containing the word and its offset in the original text.</returns>
    /// <remarks>
    /// The offset represents the zero-based starting index position of each word within the original text string.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = StringHelper.SplitByNonLetters("hello world");
    /// Returns: [("hello", 0), ("world", 6)]
    /// </code>
    /// <code>
    /// var result = StringHelper.SplitByNonLetters("  hello");
    /// Returns: [("hello", 2)]
    /// </code>
    /// <code>
    /// var result = StringHelper.SplitByNonLetters("hello, world!");
    /// Returns: [("hello", 0), ("world", 7)]
    /// </code>
    /// </example>
    public static IEnumerable<(string Word, int Offset)> SplitByNonLetters(string text)
    {
        var start = -1;
        for (var i = 0; i < text.Length; i++)
        {
            var character = text[i];

            // Do not skip words with apostrophes in the middle, e.g. "parent's", "it's", "don't".
            // But skip apostrophes in the end of a word, e.g. "parents'".
            var apostropheInTheMiddle = character == '\'' && i + 1 < text.Length && char.IsLetter(text[i + 1]);
            if (char.IsLetter(character) || apostropheInTheMiddle)
            {
                if (start < 0)
                {
                    start = i;
                }

                continue;
            }

            if (start >= 0)
            {
                yield return (text.Substring(start, i - start), start);
                start = -1;
            }
        }

        if (start >= 0)
        {
            yield return (text.Substring(start, text.Length - start), start);
        }
    }
}
