using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Rules;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.CodeFixes;

/// <summary>
/// Offers to annotate the method that stopped the search, so that INCL004 can be answered.
/// </summary>
/// <remarks>
/// A method whose source is in the solution gets the attribute on itself, <c>[PassesIncludes(nameof(query))]</c>.
/// Any other method can only be described from the project's own assembly, and that line goes into a file of
/// its own the way Visual Studio keeps "GlobalSuppressions.cs".
/// <para>
/// The analyzer has already decided what may be written, see <see cref="UnreadableMember"/>: the fix is only
/// offered when the annotation would be allowed and would let the search go on. When several parameters hold
/// the same entity, the user picks the one the entities come from.
/// </para>
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PassesIncludesCodeFixProvider))]
[Shared]
public sealed class PassesIncludesCodeFixProvider : CodeFixProvider
{
    private const string BridgesFileName = "NavigationIncludes.cs";

    private const string AttributesNamespace =
        "Saritasa.Tools.CodeAnalyzers.Abstractions.NavigationInclude.Attributes";

    private const string AttributeName = "PassesIncludes";

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
            if (BridgeRequest.Read(diagnostic.Properties) is { } request)
            {
                context.RegisterCodeFix(CreateAction(context.Document, request), diagnostic);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// One action when only one parameter holds the entities, otherwise a submenu with one action per
    /// parameter.
    /// </summary>
    private static CodeAction CreateAction(Document document, BridgeRequest request)
    {
        if (request.Parameters.Count == 1)
        {
            var from = request.Parameters[0];

            return CreateParameterAction(
                $"Mark {request.MethodName} as passing includes from '{from}'",
                document,
                request,
                from);
        }

        var nested = request.Parameters
            .Select(from => CreateParameterAction(from, document, request, from))
            .ToImmutableArray();

        return CodeAction.Create($"Mark {request.MethodName} as passing includes from", nested, isInlinable: false);
    }

    private static CodeAction CreateParameterAction(
        string title,
        Document document,
        BridgeRequest request,
        string from)
        => CodeAction.Create(
            title,
            cancellationToken => AddBridgeAsync(document, request, from, cancellationToken),
            equivalenceKey: AttributeName + ":" + request.MethodId + ":" + from + ":" + request.Callback);

    /// <summary>
    /// Annotates the method where its source can be edited, and describes it from this project otherwise.
    /// </summary>
    private static async Task<Solution> AddBridgeAsync(
        Document document,
        BridgeRequest request,
        string from,
        CancellationToken cancellationToken)
    {
        var declaration = await FindEditableDeclarationAsync(document.Project, request.MethodId, cancellationToken)
            .ConfigureAwait(false);

        return declaration is var (declaringDocument, method)
            ? await AnnotateMethodAsync(declaringDocument, method, request, from, cancellationToken)
                .ConfigureAwait(false)
            : await AddAssemblyBridgeAsync(document.Project, request, from, cancellationToken)
                .ConfigureAwait(false);
    }

    /// <summary>
    /// The declaration of the method, when its source is in the solution and its project can see the
    /// attribute. Null for a method of a package or of a project that does not reference the attributes.
    /// </summary>
    private static async Task<(Document Document, MethodDeclarationSyntax Method)?> FindEditableDeclarationAsync(
        Project project,
        string methodId,
        CancellationToken cancellationToken)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
        if (compilation is null ||
            DocumentationCommentId.GetFirstSymbolForDeclarationId(methodId, compilation) is not IMethodSymbol method)
        {
            return null;
        }

        var source = await SymbolFinder.FindSourceDefinitionAsync(method, project.Solution, cancellationToken)
            .ConfigureAwait(false) ?? method;

        foreach (var reference in source.DeclaringSyntaxReferences)
        {
            if (project.Solution.GetDocument(reference.SyntaxTree) is not { } document ||
                await reference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false)
                    is not MethodDeclarationSyntax declaration ||
                !await SeesAttributeAsync(document.Project, cancellationToken).ConfigureAwait(false))
            {
                continue;
            }

            return (document, declaration);
        }

        return null;
    }

    /// <summary>
    /// True when the project references the attributes, so that an attribute written in it compiles.
    /// </summary>
    private static async Task<bool> SeesAttributeAsync(Project project, CancellationToken cancellationToken)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

        return compilation?.GetTypeByMetadataName(AttributesNamespace + "." + AttributeName + "Attribute") is not null;
    }

    /// <summary>
    /// Puts <c>[PassesIncludes(nameof(from))]</c> on the method itself, adding the using when it is missing.
    /// </summary>
    private static async Task<Solution> AnnotateMethodAsync(
        Document document,
        MethodDeclarationSyntax method,
        BridgeRequest request,
        string from,
        CancellationToken cancellationToken)
    {
        var arguments = "nameof(" + from + ")";

        if (request.Callback is { } callback)
        {
            arguments += ", ToCallback = nameof(" + callback + ")";
        }

        if (request.CallbackPosition != 0)
        {
            arguments += ", ToCallbackParameter = " + request.CallbackPosition.ToString(CultureInfo.InvariantCulture);
        }

        // Written fully qualified and reduced afterwards, so the using is added only where it is missing, and
        // in the place the file keeps its usings.
        var attribute = SyntaxFactory.Attribute(
            SyntaxFactory.ParseName(AttributesNamespace + "." + AttributeName)
                .WithAdditionalAnnotations(Simplifier.Annotation, Simplifier.AddImportsAnnotation),
            SyntaxFactory.ParseAttributeArgumentList("(" + arguments + ")"));

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document.Project.Solution;
        }

        var newLine = await GetNewLineAsync([document], cancellationToken).ConfigureAwait(false);
        var annotated = document.WithSyntaxRoot(
            root.ReplaceNode(method, AddAttributeList(method, attribute, newLine)));

        annotated = await ImportAdder
            .AddImportsAsync(annotated, Simplifier.AddImportsAnnotation, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        annotated = await Simplifier
            .ReduceAsync(annotated, Simplifier.Annotation, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return annotated.Project.Solution;
    }

    /// <summary>
    /// Puts the attribute on a line of its own in front of the method, after its documentation and at its
    /// indentation.
    /// </summary>
    /// <remarks>
    /// The trivia is built by hand rather than left to the formatter, which would break the line with its
    /// own default instead of the one the file uses.
    /// </remarks>
    private static MethodDeclarationSyntax AddAttributeList(
        MethodDeclarationSyntax method,
        AttributeSyntax attribute,
        string newLine)
    {
        var leading = method.GetLeadingTrivia();
        var indentation = leading.Count > 0 && leading[leading.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia)
            ? SyntaxFactory.TriviaList(leading[leading.Count - 1])
            : SyntaxFactory.TriviaList();

        var list = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
            .WithLeadingTrivia(leading)
            .WithTrailingTrivia(SyntaxFactory.EndOfLine(newLine));

        var bare = method.WithLeadingTrivia(indentation);

        return bare.WithAttributeLists(bare.AttributeLists.Insert(0, list));
    }

    /// <summary>
    /// Adds the assembly attribute to the project's bridges file, creating the file when there is none yet.
    /// </summary>
    private static async Task<Solution> AddAssemblyBridgeAsync(
        Project project,
        BridgeRequest request,
        string from,
        CancellationToken cancellationToken)
    {
        var bridge = BuildAssemblyBridge(request, from);
        var existing = FindBridgesDocument(project);

        if (existing is null)
        {
            // Built through the syntax API rather than as text, and with the line breaks the project already
            // uses. Analyzers may not read Environment.NewLine.
            var newLine = await GetNewLineAsync(project.Documents, cancellationToken).ConfigureAwait(false);
            var newFile = SyntaxFactory
                .ParseCompilationUnit("using " + AttributesNamespace + ";\n" + bridge)
                .NormalizeWhitespace(eol: newLine);

            return project
                .AddDocument(BridgesFileName, newFile)
                .Project
                .Solution;
        }

        var root = await existing.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax unit)
        {
            return project.Solution;
        }

        var added = SyntaxFactory
            .ParseCompilationUnit(bridge)
            .AttributeLists
            .Select(list => list.WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));

        var fileNewLine = await GetNewLineAsync([existing], cancellationToken).ConfigureAwait(false);

        return existing
            .WithSyntaxRoot(unit.AddAttributeLists(added.ToArray()).NormalizeWhitespace(eol: fileNewLine))
            .Project
            .Solution;
    }

    /// <summary>
    /// The line break the first of the documents that has one uses, CRLF when none has.
    /// </summary>
    private static async Task<string> GetNewLineAsync(
        IEnumerable<Document> documents,
        CancellationToken cancellationToken)
    {
        foreach (var document in documents)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newLine = root?
                .DescendantTrivia()
                .FirstOrDefault(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

            if (newLine is { Span.Length: > 0 } found)
            {
                return found.ToString();
            }
        }

        return "\r\n";
    }

    /// <summary>
    /// Builds the assembly attribute line, e.g.
    /// <c>[assembly: PassesIncludes(typeof(SomeLib.Ext), "ForEachItem", "items", ToCallback = "action")]</c>.
    /// </summary>
    private static string BuildAssemblyBridge(BridgeRequest request, string from)
    {
        var arguments = "typeof(" + request.TypeName + "), \"" + request.MethodName + "\", \"" + from + "\"";

        if (request.Callback is { } callback)
        {
            arguments += ", ToCallback = \"" + callback + "\"";
        }

        if (request.CallbackPosition != 0)
        {
            arguments += ", ToCallbackParameter = " + request.CallbackPosition.ToString(CultureInfo.InvariantCulture);
        }

        return "[assembly: " + AttributeName + "(" + arguments + ")]";
    }

    /// <summary>
    /// The file this project already keeps its bridges in, or null when there is none.
    /// </summary>
    private static Document? FindBridgesDocument(Project project)
        => project.Documents.FirstOrDefault(document =>
            string.Equals(document.Name, BridgesFileName, StringComparison.Ordinal));

    /// <summary>
    /// What the analyzer put on the diagnostic: the method, the parameters the entities may come from, and the
    /// callback they arrived through, if any.
    /// </summary>
    private sealed class BridgeRequest
    {
        private BridgeRequest(
            string typeName,
            string methodName,
            string methodId,
            IReadOnlyList<string> parameters,
            string? callback,
            int callbackPosition)
        {
            TypeName = typeName;
            MethodName = methodName;
            MethodId = methodId;
            Parameters = parameters;
            Callback = callback;
            CallbackPosition = callbackPosition;
        }

        /// <summary>
        /// Declaring type, as written inside "typeof(...)".
        /// </summary>
        public string TypeName { get; }

        public string MethodName { get; }

        /// <summary>
        /// Documentation id of the method, to find it again in the solution.
        /// </summary>
        public string MethodId { get; }

        /// <summary>
        /// Parameters the entities may come from, at least one.
        /// </summary>
        public IReadOnlyList<string> Parameters { get; }

        /// <summary>
        /// Callback parameter the entities arrived through, or null when they arrived at the result.
        /// </summary>
        public string? Callback { get; }

        /// <summary>
        /// Position inside the callback, zero when there is none.
        /// </summary>
        public int CallbackPosition { get; }

        /// <summary>
        /// Reads the request, or returns null when the analyzer offered nothing to write.
        /// </summary>
        public static BridgeRequest? Read(ImmutableDictionary<string, string?> properties)
        {
            if (!properties.TryGetValue(UnreadableMember.TypeKey, out var typeName) ||
                !properties.TryGetValue(UnreadableMember.MethodKey, out var methodName) ||
                !properties.TryGetValue(UnreadableMember.MethodIdKey, out var methodId) ||
                !properties.TryGetValue(UnreadableMember.ParametersKey, out var parameterNames) ||
                string.IsNullOrEmpty(typeName) ||
                string.IsNullOrEmpty(methodName) ||
                string.IsNullOrEmpty(methodId) ||
                string.IsNullOrEmpty(parameterNames))
            {
                return null;
            }

            properties.TryGetValue(UnreadableMember.CallbackKey, out var callback);
            properties.TryGetValue(UnreadableMember.CallbackPositionKey, out var position);

            return new BridgeRequest(
                typeName!,
                methodName!,
                methodId!,
                parameterNames!.Split(UnreadableMember.ParameterSeparator),
                string.IsNullOrEmpty(callback) ? null : callback,
                int.TryParse(position, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0);
        }
    }
}
