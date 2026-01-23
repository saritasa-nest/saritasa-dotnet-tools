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
    /// Creates spell checker from additional files.
    /// </summary>
    /// <param name="files">Dictionary files. Should contain <c>.dic</c>, <c>.aff</c> and <c>exclusions.txt</c> files.</param>
    /// <returns>Spell checker.</returns>
    public static SpellChecker Create(IEnumerable<AdditionalText> files)
    {
        var customWords = ImmutableHashSet.CreateBuilder<string>(StringComparer.OrdinalIgnoreCase);
        WordList? wordList = null;

        var additionalFiles = files.ToList();
        var dicFile = additionalFiles.FirstOrDefault(f => f.Path.EndsWith(".dic", StringComparison.OrdinalIgnoreCase));

        if (dicFile != null)
        {
            var dicSourceText = dicFile.GetText();
            if (dicSourceText != null)
            {
                var affFile = additionalFiles.FirstOrDefault(f => f.Path.EndsWith(".aff", StringComparison.OrdinalIgnoreCase));
                var affSourceText = affFile?.GetText();

                using var dicStream = new MemoryStream();
                var dicStreamWriter = new StreamWriter(dicStream);
                dicSourceText.Write(dicStreamWriter);
                dicStreamWriter.Flush();
                dicStream.Position = 0;

                if (affSourceText != null)
                {
                    using var affStream = new MemoryStream();
                    var affStreamWriter = new StreamWriter(affStream);
                    affSourceText.Write(affStreamWriter);
                    affStreamWriter.Flush();
                    affStream.Position = 0;
                    wordList = WordList.CreateFromStreams(dicStream, affStream);
                }
                else
                {
                    wordList = WordList.CreateFromStreams(dicStream, new MemoryStream());
                }
            }
        }

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

        return new SpellChecker(wordList, customWords.ToImmutable());
    }
}
