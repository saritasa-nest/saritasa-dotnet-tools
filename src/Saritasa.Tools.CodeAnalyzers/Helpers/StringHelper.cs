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
