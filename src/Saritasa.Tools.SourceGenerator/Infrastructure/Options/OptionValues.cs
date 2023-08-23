namespace Saritasa.Tools.SourceGenerator.Infrastructure.Options;

/// <summary>
/// Represents an application default option values.
/// </summary>
internal static class OptionValues
{
    /// <summary>
    /// Default indentation style.
    /// </summary>
    public const IndentStyle DefaultIndentStyle = IndentStyle.Space;

    /// <summary>
    /// Default indentation size.
    /// </summary>
    public const int DefaultIndentSize = 4;

    /// <summary>
    /// Default backing field naming convention.
    /// </summary>
    public const NamingConvention DefaultBackingFieldNamingConvention = NamingConvention.CamelCase;

    /// <summary>
    /// Default value which indicates whether backing field should use underscore.
    /// </summary>
    public const bool DefaultBackingFieldShouldUseUnderscore = false;

    /// <summary>
    /// Default property changed raise method names.
    /// </summary>
    public static readonly string[] PropertyChangedNames = new[] { "OnPropertyChanged", "RaisePropertyChanged" };

    /// <summary>
    /// Default property changing raise method names.
    /// </summary>
    public static readonly string[] PropertyChangingNames = new[] { "OnPropertyChanging", "RaisePropertyChanging" };
}
