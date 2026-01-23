using System.Reflection;
using Microsoft.CodeAnalysis;
using WeCantSpell.Hunspell;

namespace Saritasa.Tools.CodeAnalyzers.Helpers;

/// <summary>
/// Spell checker.
/// </summary>
public static class SpellChecker
{
    private const string DefaultDicFileName = "en.dic";
    private const string DefaultAffFileName = "en.aff";

    /// <summary>
    /// Creates spell checker from packaged dictionary files and optional additional files.
    /// </summary>
    /// <param name="files">Optional additional files for custom words (e.g., <c>exclusions.txt</c>).</param>
    /// <returns>Spell checker.</returns>
    public static WordList? CreateWordList(IEnumerable<AdditionalText> files)
    {
        var wordList = LoadWordListFromEmbeddedResources();

        if (wordList is null)
        {
            return null;
        }

        AddExclusions(files, wordList);

        return wordList;
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

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
    private static WordList? LoadWordListFromEmbeddedResources()
    {
        try
        {
            var assembly = typeof(SpellChecker).Assembly;

            var dicStream = TryOpenResourceStream(assembly, DefaultDicFileName);
            if (dicStream is null)
            {
                return null;
            }

            using (dicStream)
            {
                var affStream = TryOpenResourceStream(assembly, DefaultAffFileName);
                if (affStream is null)
                {
                    return null;
                }

                using (affStream)
                {
                    return WordList.CreateFromStreams(dicStream, affStream);
                }
            }
        }
        catch
        {
            return null;
        }
    }

    private static Stream? TryOpenResourceStream(Assembly assembly, string fileName)
    {
        var match = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        return match is null ? null : assembly.GetManifestResourceStream(match);
    }
#pragma warning restore RS1035
}
