using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using WeCantSpell.Hunspell;

namespace Saritasa.Tools.CodeAnalyzers.Helpers;

/// <summary>
/// Spell checker.
/// </summary>
public class SpellChecker
{
    private readonly WordList? wordList;
    private readonly ImmutableHashSet<string> customWords;

    private SpellChecker(WordList? wordList, ImmutableHashSet<string> customWords)
    {
        this.wordList = wordList;
        this.customWords = customWords;
    }

    /// <summary>
    /// Indicates that spell checker has no words to check against.
    /// </summary>
    public bool IsEmpty => wordList == null && customWords.IsEmpty;

    /// <summary>
    /// Checks whether word is spelled correctly. It uses custom words then built-in word list.
    /// </summary>
    /// <param name="word">Word to check.</param>
    /// <returns>True if the word is spelled correctly.</returns>
    public bool Check(string word)
    {
        if (customWords.Contains(word))
        {
            return true;
        }

        return wordList?.Check(word) ?? false;
    }

    /// <summary>
    /// Creates spell checker from packaged dictionary files and optional additional files.
    /// </summary>
    /// <param name="files">Optional additional files for custom words (e.g., <c>exclusions.txt</c>).</param>
    /// <returns>Spell checker.</returns>
    public static SpellChecker Create(IEnumerable<AdditionalText> files)
    {
        var customWords = ImmutableHashSet.CreateBuilder<string>(StringComparer.OrdinalIgnoreCase);

        // Load dictionary files from analyzer assembly directory
        var wordList = LoadWordListFromAssemblyDirectory();

        // Load custom words from additional files (if provided by user projects)
        var additionalFiles = files.ToList();
        foreach (var file in additionalFiles)
        {
            var path = file.Path;
            if (string.IsNullOrWhiteSpace(path) || !path.Contains("words", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (path.EndsWith(".dic", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".aff", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var text = file.GetText();
            if (text is null)
            {
                continue;
            }

            foreach (var line in text.Lines)
            {
                var word = line.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(word))
                {
                    customWords.Add(word);
                }
            }
        }

        // Also load custom word files from analyzer directory
        LoadCustomWordsFromAssemblyDirectory(customWords);

        return new SpellChecker(wordList, customWords.ToImmutable());
    }

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
    private static WordList? LoadWordListFromAssemblyDirectory()
    {
        try
        {
            var analyzerLocation = typeof(SpellChecker).Assembly.Location;
            var analyzerDir = Path.GetDirectoryName(analyzerLocation);
            if (string.IsNullOrWhiteSpace(analyzerDir))
            {
                return null;
            }

            var dicFile = Path.Combine(analyzerDir, "en.dic");
            var affFile = Path.Combine(analyzerDir, "en.aff");

            if (!File.Exists(dicFile))
            {
                return null;
            }

            if (File.Exists(affFile))
            {
                return WordList.CreateFromFiles(dicFile, affFile);
            }

            return WordList.CreateFromFiles(dicFile);
        }
        catch
        {
            return null;
        }
    }

    private static void LoadCustomWordsFromAssemblyDirectory(ImmutableHashSet<string>.Builder customWords)
    {
        try
        {
            var analyzerLocation = typeof(SpellChecker).Assembly.Location;
            var analyzerDir = Path.GetDirectoryName(analyzerLocation);
            if (string.IsNullOrWhiteSpace(analyzerDir))
            {
                return;
            }

            var txtFiles = Directory.GetFiles(analyzerDir, "*.txt");
            foreach (var txtFile in txtFiles)
            {
                var lines = File.ReadAllLines(txtFile);
                foreach (var line in lines)
                {
                    var word = line.Trim();
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        customWords.Add(word);
                    }
                }
            }
        }
        catch
        {
            // Silently ignore errors loading custom word files
        }
    }
#pragma warning restore RS1035
}
