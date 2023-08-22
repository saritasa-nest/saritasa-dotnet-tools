using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Saritasa.Tools.SourceGenerator.Abstractions.Syntax;
using Saritasa.Tools.SourceGenerator.Infrastructure;
using Saritasa.Tools.SourceGenerator.Models.Analyzers;
using Saritasa.Tools.SourceGenerator.Models.Metadata;
using Saritasa.Tools.SourceGenerator.Models.Nodes;
using Saritasa.Tools.SourceGenerator.Utils;

namespace Saritasa.Tools.SourceGenerator.Builders;

/// <summary>
/// Partial class builder based on <see cref="ClassAnalysis"/>.
/// </summary>
public class ClassBuilder : ISyntaxBuilder<ClassMetadata, ClassAnalysis>
{
    private static readonly ISyntaxBuilder<PropertyMetadata, InterfaceAnalysis> propertyChangedPropertyBuilder
        = new InterfacePropertyBuilder(propertyName: "PropertyChanged", typeof(PropertyChangedEventHandler));

    private static readonly ISyntaxBuilder<PropertyMetadata, InterfaceAnalysis> propertyChangingPropertyBuilder
        = new InterfacePropertyBuilder(propertyName: "PropertyChanging", typeof(PropertyChangingEventHandler));

    private readonly OptionsManager optionsManager;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="optionsManager">Options manager.</param>
    public ClassBuilder(OptionsManager optionsManager)
    {
        this.optionsManager = optionsManager;
    }

    /// <inheritdoc/>
    public ClassMetadata Build(ClassAnalysis analysis)
    {
        var members = new List<MemberMetadata>();
        var methods = new List<MethodMetadata>();

        var declaredMethods = SymbolUtils.GetMembers(analysis.Symbol).OfType<IMethodSymbol>();
        var declaredMethodNames = declaredMethods.Select(method => method.Name);

        var propertyChanged = GetInterfaceMethodInvocation(
            analysis.Symbol,
            analysis.PropertyChanged,
            analysis =>
            {
                var property = propertyChangedPropertyBuilder.Build(analysis);
                members.Add(property);
                return property;
            },
            (analysis, invocation) =>
            {
                invocation.Arguments.Add("this");
                invocation.Arguments.Add("new PropertyChangedEventArgs(propertyName)");

                var propertyChangedMethodName = optionsManager.PropertyChangedOptions
                    .MethodNames
                    .Except(declaredMethodNames)
                    .First();

                var method = new InterfaceMethodBuilder(
                    methodName: propertyChangedMethodName,
                    invocation).Build(analysis);
                methods.Add(method);
                return method;
            });

        var propertyChanging = GetInterfaceMethodInvocation(
            analysis.Symbol,
            analysis.PropertyChanging,
            analysis =>
            {
                var property = propertyChangingPropertyBuilder.Build(analysis);
                members.Add(property);
                return property;
            },
            (analysis, invocation) =>
            {
                invocation.Arguments.Add("this");
                invocation.Arguments.Add("new PropertyChangingEventArgs(propertyName)");

                var propertyChangingMethodName = optionsManager.PropertyChangingOptions
                    .MethodNames
                    .Except(declaredMethodNames)
                    .First();

                var method = new InterfaceMethodBuilder(
                    methodName: propertyChangingMethodName,
                    invocation).Build(analysis);
                methods.Add(method);
                return method;
            });

        var additionalMembers = GetMembersMetadata(
                analysis.Properties,
                analysis.Fields,
                propertyChanged,
                propertyChanging);
        members.AddRange(additionalMembers);

        return new ClassMetadata()
        {
            Name = analysis.Name,
            Namespace = analysis.Namespace,
            Modifier = MemberModifiers.Public,
            Methods = methods,
            Members = members,
        };
    }

    private static InvocationMethodMetadata? GetInterfaceMethodInvocation(
        ITypeSymbol symbol,
        InterfaceAnalysis analysis,
        Func<InterfaceAnalysis, PropertyMetadata> buildProperty,
        Func<InterfaceAnalysis, InvocationMethodMetadata, MethodMetadata> buildMethod)
    {
        if (!analysis.HasInterface)
        {
            return default;
        }

        var propertyChanged = InvocationMethodMetadata.Instance;

        if (analysis.MethodSymbol is not null)
        {
            propertyChanged.Name = analysis.MethodSymbol.Name;
        }
        else
        {
            var shouldBuildProperty = analysis.EventSymbol == null ||
                SymbolUtils.ContainsBaseList(symbol, analysis.EventSymbol.ContainingSymbol);

            var propertyInvocation = new InvocationMethodMetadata
            {
                Name = shouldBuildProperty
                    ? buildProperty(analysis).Name
                    : analysis.EventSymbol.Name,
                IsNullable = true,
                IsDelegate = true,
            };
            var method = buildMethod(analysis, propertyInvocation);
            propertyChanged.Name = method.Name;
        }

        return propertyChanged;
    }

    private static IReadOnlyCollection<MemberMetadata> GetMembersMetadata(
        IReadOnlyCollection<PropertyAnalysis> properties,
        IReadOnlyCollection<FieldAnalysis> fields,
        InvocationMethodMetadata? propertyChanged = null,
        InvocationMethodMetadata? propertyChanging = null)
    {
        var propertyNames = properties.Select(prop => PropertyUtils.Lowercase(prop.Name));
        var fieldsToAnalyze = fields
            .Where(field => !propertyNames.Contains(field.Name))
            .Where(field => !field.DoNotNotify);

        var propertyBuilder = new PropertyBuilder(propertyChanged, propertyChanging);
        var members = new List<MemberMetadata>();
        foreach (var fieldAnalysis in fieldsToAnalyze)
        {
            var member = propertyBuilder.Build(fieldAnalysis);
            members.Add(member);
        }
        return members;
    }
}
