namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

/// <summary>
/// One line of <see cref="BuiltInDeclarations"/>, before it is matched against the types of a compilation.
/// </summary>
internal readonly struct BuiltInDeclaration
{
    /// <summary>
    /// Initializes the line.
    /// </summary>
    /// <param name="type">Metadata name of the type that declares the member.</param>
    /// <param name="name">Name of the method or property.</param>
    /// <param name="from">Parameter the entities come from, empty for the value the member is used on.</param>
    /// <param name="toLambda">Parameter that takes the callback, or null when the line describes the result.</param>
    /// <param name="toLambdaParameter">Position of the callback's own parameter.</param>
    /// <param name="notWhen">Parameter whose presence means this overload hands back something else.</param>
    public BuiltInDeclaration(
        string type,
        string name,
        string from,
        string? toLambda,
        int toLambdaParameter,
        string? notWhen)
    {
        Type = type;
        Name = name;
        From = from;
        ToLambda = toLambda;
        ToLambdaParameter = toLambdaParameter;
        NotWhen = notWhen;
    }

    /// <summary>
    /// Metadata name of the type that declares the member.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Name of the method or property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Parameter the entities come from, empty for the value the member is used on.
    /// </summary>
    public string From { get; }

    /// <summary>
    /// Parameter that takes the callback, or null when the line describes the result.
    /// </summary>
    public string? ToLambda { get; }

    /// <summary>
    /// Position of the callback's own parameter the entities arrive at.
    /// </summary>
    public int ToLambdaParameter { get; }

    /// <summary>
    /// Parameter whose presence means this overload hands back something else.
    /// </summary>
    /// <remarks>
    /// TODO SN-993: matching an overload by one parameter name is fragile and unverified, see
    /// <see cref="BuiltInDeclarations"/>.
    /// </remarks>
    public string? NotWhen { get; }
}
