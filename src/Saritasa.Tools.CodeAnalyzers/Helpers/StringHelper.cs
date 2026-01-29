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
}

