using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Bridging;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.EntityFramework;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Requirements;
using Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Search;

namespace Saritasa.Tools.CodeAnalyzers.Analyzers.NavigationInclude.Aliasing;

/// <summary>
/// Reads one graph's statements for the alias picture: which objects an expression gives, and how a statement
/// changes the picture.
/// </summary>
/// <remarks>
/// A query, a call of an <c>[Includes]</c> method and an element of a collection give a new object; a bridge that
/// returns the value it gets, such as a <c>[PassesIncludes]</c> method, gives the objects of that value; a field, a
/// static and a call nobody annotated give unknown objects. Called methods change nothing.
/// </remarks>
internal sealed class AliasEvaluator
{
    private readonly ILookup<CaptureId, IOperation> capturedValues;

    /// <summary>
    /// Initializes the evaluator.
    /// </summary>
    /// <param name="flowGraph">Graph whose statements are read.</param>
    public AliasEvaluator(FlowGraph flowGraph)
    {
        capturedValues = flowGraph.Graph.Blocks
            .SelectMany(block => block.Operations)
            .OfType<IFlowCaptureOperation>()
            .ToLookup(capture => capture.Id, capture => capture.Value);
    }

    /// <summary>
    /// Returns the picture after the statement at the position runs.
    /// </summary>
    /// <param name="state">Picture before the statement.</param>
    /// <param name="statement">Position of the statement.</param>
    /// <returns>New picture.</returns>
    public AliasState Apply(AliasState state, CodePosition statement)
    {
        if (statement.Statement is not { } statementOperation)
        {
            return state;
        }

        var operation = statementOperation is IExpressionStatementOperation expressionStatement
            ? expressionStatement.Operation
            : statementOperation;

        state = operation switch
        {
            // "#1 = ...": a temporary value the compiler creates for ?:, ??, object initializers and foreach.
            IFlowCaptureOperation capture
                => state.AssignVariable(capture.Id, Evaluate(capture.Value, state, statement)),

            ISimpleAssignmentOperation assignment
                => Assign(state, assignment.Target, Evaluate(assignment.Value, state, statement), statement),

            // "var (id, user) = pair": each part is an element of the pair, so a new object.
            IDeconstructionAssignmentOperation deconstruction
                => AssignParts(state, deconstruction.Target),

            _ => state,
        };

        return AssignOutArguments(state, statementOperation);
    }

    /// <summary>
    /// Returns the objects the expression gives at the position.
    /// </summary>
    /// <param name="expression">Expression.</param>
    /// <param name="state">Picture at the position.</param>
    /// <param name="position">Position of the statement that contains the expression.</param>
    /// <returns>Objects.</returns>
    public ObjectSet Evaluate(IOperation expression, AliasState state, CodePosition position)
    {
        var value = Value.Create(expression, position);
        var operation = value.Operation;

        return operation switch
        {
            // "user with { ... }": a record copied into a new object. The compiler lowers it to a call of the
            // record's clone method, so it is recognized by its syntax.
            _ when operation.Syntax is WithExpressionSyntax
                => Created(operation),

            // "user", "#1".
            _ when WritesWalker.GetVariable(operation) is { } variable
                => state.Read(Slot.OfVariable(variable)),

            // "null", "default".
            { ConstantValue: { HasValue: true, Value: null } }
                => ObjectSet.Empty,

            IObjectCreationOperation or IAnonymousObjectCreationOperation or IArrayCreationOperation
                => Created(operation),

            // "a = b = c": the value of an assignment is the assigned value.
            ISimpleAssignmentOperation assignment
                => Evaluate(assignment.Value, state, position),

            // "users[0]", "enumerator.Current", "pair.Value": a bridge moves to an element, which is a new object.
            IPropertyReferenceOperation or IArrayElementReferenceOperation
                when BridgeCrosser.FromValue(value).Count > 0 || operation is IArrayElementReferenceOperation
                => Created(operation),

            // "user.Profile": the Profile slot of each object user may be. An indexer reads an element.
            IPropertyReferenceOperation { Instance: { } instance, Property: var property } reference
                => property.IsIndexer
                    ? Created(reference)
                    : state.ReadProperty(Evaluate(instance, state, position), property),

            IInvocationOperation call
                => EvaluateCall(call, value, state, position),

            // A field, a static, anything else: the object cannot be named.
            _ => ObjectSet.Unknown,
        };
    }

    private static ObjectSet Created(IOperation operation) => ObjectSet.Of(AliasObject.CreatedBy(operation));

    /// <summary>
    /// The objects a call gives.
    /// </summary>
    private ObjectSet EvaluateCall(IInvocationOperation call, Value value, AliasState state, CodePosition position)
    {
        // "query.Include(...)" and a call of an [Includes] method load new objects.
        if (EfIncludeReader.ReadInclude(call) is not null ||
            EfIncludeReader.ReadThenInclude(call) is not null ||
            AttributeReader.MethodHasAnyIncludesAttribute(call.TargetMethod))
        {
            return Created(call);
        }

        var crossings = BridgeCrosser.FromValue(value);

        if (crossings.Count == 0)
        {
            return ObjectSet.Unknown;
        }

        // "Reload(user)" or "query.Where(...)": the value comes back as it went in. A bridge that changes the type,
        // such as "users.First()", or that moves to a property, such as "users.Select(u => u.Profile)", gives
        // an element, which is a new object.
        var returnsSource = crossings.All(crossing =>
            crossing.Prefix.Count == 0 &&
            SymbolEqualityComparer.Default.Equals(crossing.Source.Type, call.Type));

        if (!returnsSource)
        {
            return Created(call);
        }

        var objects = ObjectSet.Empty;

        foreach (var crossing in crossings)
        {
            objects = objects.Union(Evaluate(crossing.Source, state, position));
        }

        return objects;
    }

    /// <summary>
    /// The picture after <c>target = value</c>.
    /// </summary>
    private AliasState Assign(AliasState state, IOperation target, ObjectSet value, CodePosition statement)
    {
        // "user.Profile = new UserProfile { ... }" becomes "#1 = user.Profile; ...; #1 = #2": the temporary
        // stands for the property it captured.
        if (target is IFlowCaptureReferenceOperation reference)
        {
            var captured = capturedValues[reference.Id].ToList();

            if (captured.Count == 1 && captured[0] is IPropertyReferenceOperation capturedProperty)
            {
                target = capturedProperty;
            }
        }

        if (WritesWalker.GetVariable(target) is { } variable)
        {
            return state.AssignVariable(variable, value);
        }

        // An indexer writes an element, which is not stored.
        if (target is IPropertyReferenceOperation { Instance: { } instance, Property: { IsIndexer: false } property })
        {
            return state.AssignProperty(Evaluate(instance, state, statement), property, value);
        }

        return state;
    }

    private static AliasState AssignParts(AliasState state, IOperation target)
    {
        foreach (var part in target.DescendantsAndSelf())
        {
            if (WritesWalker.GetVariable(part) is { } variable)
            {
                state = state.AssignVariable(variable, Created(part));
            }
        }

        return state;
    }

    /// <summary>
    /// "dictionary.TryGetValue(id, out var user)": user gets an element, which is a new object. A ref argument may
    /// get anything.
    /// </summary>
    private static AliasState AssignOutArguments(AliasState state, IOperation statement)
    {
        foreach (var argument in statement.DescendantsAndSelf().OfType<IArgumentOperation>())
        {
            if (argument.Parameter?.RefKind is not (RefKind.Out or RefKind.Ref) ||
                WritesWalker.GetVariable(argument.Value) is not { } variable)
            {
                continue;
            }

            state = state.AssignVariable(
                variable,
                argument.Parameter.RefKind == RefKind.Out ? Created(argument) : ObjectSet.Unknown);
        }

        return state;
    }
}
