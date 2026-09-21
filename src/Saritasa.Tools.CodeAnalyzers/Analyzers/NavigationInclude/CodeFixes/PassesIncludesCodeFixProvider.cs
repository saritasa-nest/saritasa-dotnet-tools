using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Services;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.CodeFixes;

/// <summary>
/// Offers to declare the method that stopped the search, so that INCL004 can be answered.
/// </summary>
/// <remarks>
/// The declaration is written on the project's own assembly, which is the only way to describe a method of a
/// library the project does not own. It goes into a file of its own, the way Visual Studio keeps suppressions
/// in "GlobalSuppressions.cs".
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PassesIncludesCodeFixProvider))]
[Shared]
public sealed class PassesIncludesCodeFixProvider : CodeFixProvider
{
    private const string DeclarationsFileName = "NavigationIncludes.cs";

    private const string AttributesNamespace =
        "Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(NavigationIncludeRulesProvider.Incl4IdCannotCheckNavigationProperty);

    /// <inheritdoc />
    public override FixAllProvider? GetFixAllProvider() => null;

    /// <inheritdoc />
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            if (!diagnostic.Properties.TryGetValue(UnreadableMember.TypeKey, out var typeName) ||
                !diagnostic.Properties.TryGetValue(UnreadableMember.MethodKey, out var methodName) ||
                string.IsNullOrEmpty(typeName) ||
                string.IsNullOrEmpty(methodName))
            {
                continue;
            }

            diagnostic.Properties.TryGetValue(UnreadableMember.ParameterKey, out var parameterName);

            var title = string.IsNullOrEmpty(parameterName)
                ? $"Mark {methodName} as handing back the entities it holds"
                : $"Mark {methodName} as handing back the entities of '{parameterName}'";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    cancellationToken => AddDeclarationAsync(
                        context.Document.Project,
                        typeName!,
                        methodName!,
                        parameterName,
                        cancellationToken),
                    equivalenceKey: typeName + "." + methodName),
                diagnostic);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds the declaration to the project's declarations file, creating the file when there is none yet.
    /// </summary>
    private static async Task<Solution> AddDeclarationAsync(
        Project project,
        string typeName,
        string methodName,
        string? parameterName,
        CancellationToken cancellationToken)
    {
        var declaration = BuildDeclaration(typeName, methodName, parameterName);
        var existing = FindDeclarationsDocument(project);

        if (existing is null)
        {
            // Built through the syntax API rather than as text, so that the line breaks are the ones Roslyn
            // writes everywhere else. Analyzers may not read Environment.NewLine.
            var newFile = SyntaxFactory
                .ParseCompilationUnit("using " + AttributesNamespace + ";\n" + declaration)
                .NormalizeWhitespace();

            return project
                .AddDocument(DeclarationsFileName, newFile)
                .Project
                .Solution;
        }

        var root = await existing.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax unit)
        {
            return project.Solution;
        }

        var added = SyntaxFactory
            .ParseCompilationUnit(declaration)
            .AttributeLists
            .Select(list => list.WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));

        return existing
            .WithSyntaxRoot(unit.AddAttributeLists(added.ToArray()).NormalizeWhitespace())
            .Project
            .Solution;
    }

    /// <summary>
    /// Builds the assembly attribute line, e.g.
    /// <c>[assembly: PassesIncludes(typeof(SomeLib.Ext), "Paginate", "query")]</c>.
    /// </summary>
    private static string BuildDeclaration(string typeName, string methodName, string? parameterName)
    {
        var arguments = "typeof(" + typeName + "), \"" + methodName + "\"";

        if (!string.IsNullOrEmpty(parameterName))
        {
            arguments += ", \"" + parameterName + "\"";
        }

        return "[assembly: PassesIncludes(" + arguments + ")]";
    }

    /// <summary>
    /// The file this project already keeps its declarations in, or null when there is none.
    /// </summary>
    private static Document? FindDeclarationsDocument(Project project)
        => project.Documents.FirstOrDefault(document =>
            string.Equals(document.Name, DeclarationsFileName, StringComparison.Ordinal));
}
