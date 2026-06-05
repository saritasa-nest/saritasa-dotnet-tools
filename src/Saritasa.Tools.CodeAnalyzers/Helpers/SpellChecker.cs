using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using WeCantSpell.Hunspell;

namespace Saritasa.Tools.CodeAnalyzers.Helpers;

/// <summary>
/// Spell checker.
/// </summary>
public static class SpellChecker
{
    private const string DefaultDicFileName = "en-us.dic";
    private const string DefaultAffFileName = "en-us.aff";
    private const string TechNamesFileName = "tech.names.txt";
    private const string ExclusionsFileName = "exclusions.txt";

    private static readonly Assembly assembly = typeof(SpellChecker).Assembly;

    private static readonly string[] handledDictionaryFiles =
    [
        DefaultDicFileName,
        DefaultAffFileName,
        TechNamesFileName,
        ExclusionsFileName
    ];

    /// <summary>
    /// Creates word list from packaged dictionary files and optional additional files.
    /// </summary>
    /// <param name="files">Optional additional files for custom words (e.g., <c>exclusions.txt</c>).</param>
    /// <returns>Word list.</returns>
    public static WordList CreateWordList(IEnumerable<AdditionalText> files)
    {
        var wordList = CreateWordListFromEmbeddedResources();

        AddGeneralExclusions(wordList);
        AddExclusions(files, wordList);

        return wordList;
    }

    private static WordList CreateWordListFromEmbeddedResources()
    {
        var dicStream = TryOpenResourceStream(DefaultDicFileName);
        if (dicStream is null)
        {
            throw new InvalidOperationException($"Could not find dictionary resource '{DefaultDicFileName}'.");
        }

        using (dicStream)
        {
            var affStream = TryOpenResourceStream(DefaultAffFileName);
            if (affStream is null)
            {
                throw new InvalidOperationException($"Could not find affixes resource '{DefaultAffFileName}'.");
            }

            using (affStream)
            {
                return WordList.CreateFromStreams(dicStream, affStream);
            }
        }
    }

    private static void AddGeneralExclusions(WordList wordList)
    {
        var resourceNames = assembly
            .GetManifestResourceNames()
            .Where(name => name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && !IsDictionaryHandled(name))
            .ToList();

        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                continue;
            }

            using var reader = new StreamReader(stream);
            while (reader.ReadLine() is { } line)
            {
                var word = line.Trim();
                if (!string.IsNullOrWhiteSpace(word))
                {
                    wordList.Add(word);
                }
            }
        }
    }

    private static bool IsDictionaryHandled(string name) =>
        handledDictionaryFiles.Any(handledFile => name.EndsWith(handledFile, StringComparison.OrdinalIgnoreCase));

    private static void AddExclusions(IEnumerable<AdditionalText> files, WordList wordList)
    {
        var exclusionsFile = files
            .FirstOrDefault(file => file.Path.EndsWith(ExclusionsFileName, StringComparison.OrdinalIgnoreCase));
        var text = exclusionsFile?.GetText();

        if (text is null)
        {
            return;
        }

        foreach (var line in text.Lines)
        {
            var word = line.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(word))
            {
                wordList.Add(word);
            }
        }
    }

    /// <summary>
    /// Builds a regex that matches any of the provided names.
    /// </summary>
    /// <remarks>
    /// Only the first character is case-flexible (to allow camelCase identifiers that start with lowercase).
    /// The rest of the name must match the original case exactly to prevent false matches.
    /// </remarks>
    public static Regex? BuildNamesRegex(WordList wordList)
    {
        var names = GetNames(wordList);

        if (names.Count == 0)
        {
            return null;
        }

        var patterns = names.Select(name =>
        {
            var firstLetterUpper = char.ToUpperInvariant(name[0]);
            var firstLetterLower = char.ToLowerInvariant(name[0]);
            var firstLetter = firstLetterUpper != firstLetterLower
                ? $"[{firstLetterUpper}{firstLetterLower}]"
                : Regex.Escape(name[0].ToString());
            return firstLetter + Regex.Escape(name.Substring(1));
        });

        var pattern = string.Join("|", patterns);

        return new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

    /// <summary>
    /// Gets a list of names that should be masked before spell checking to prevent incorrect camelCase splitting.
    /// </summary>
    /// <returns>List of names.</returns>
    private static List<string> GetNames(WordList wordList)
    {
        List<string> names = [];

        using var stream = TryOpenResourceStream(TechNamesFileName);
        if (stream is null)
        {
            return names;
        }

        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            var name = line.Trim();
            if (StringHelper.IsCamelCaseWord(name))
            {
                names.Add(name);
            }
            else
            {
                // When name does not have camelCase we check it as usual.
                wordList.Add(name);
            }
        }

        return names;
    }

    private static Stream? TryOpenResourceStream(string fileName)
    {
        var match = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        return match is null ? null : assembly.GetManifestResourceStream(match);
    }
}
