using System.Reflection;
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

    /// <summary>
    /// Creates word list from packaged dictionary files and optional additional files.
    /// </summary>
    /// <param name="files">Optional additional files for custom words (e.g., <c>exclusions.txt</c>).</param>
    /// <returns>Word list.</returns>
    public static WordList CreateWordList(IEnumerable<AdditionalText> files)
    {
        var wordList = CreateWordListFromEmbeddedResources();

        AddExclusions(files, wordList);

        return wordList;
    }

    private static WordList CreateWordListFromEmbeddedResources()
    {
        var assembly = typeof(SpellChecker).Assembly;

        var dicStream = TryOpenResourceStream(assembly, DefaultDicFileName);
        if (dicStream is null)
        {
            throw new InvalidOperationException($"Could not find dictionary resource '{DefaultDicFileName}'.");
        }

        using (dicStream)
        {
            var affStream = TryOpenResourceStream(assembly, DefaultAffFileName);
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

    private static Stream? TryOpenResourceStream(Assembly assembly, string fileName)
    {
        var match = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        return match is null ? null : assembly.GetManifestResourceStream(match);
    }

    private static void AddExclusions(IEnumerable<AdditionalText> files, WordList wordList)
    {
        var exclusionsFile = files.FirstOrDefault(file =>
            file.Path.Contains("exclusions") && (file.Path.EndsWith(".txt") || file.Path.EndsWith(".dic")));

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
}
