using System.Reflection;
using System.Resources;

namespace Saritasa.Tools.Domain.Localization;

public class SystemResourceLocalizer
{
    private readonly FieldInfo resourceManagerField;
    private readonly Type defaultResourceSource;

    public static SystemResourceLocalizer ComponentModelAnnotations()
    {
        // Ensure overridable assembly is loaded.
        var test = typeof(global::System.ComponentModel.DataAnnotations.RequiredAttribute);

        // Get overridable resource source.
        var srType = test.Assembly.GetType("System.SR");

        // Get overridable resource manager field setter.
        var resourceManagerField = srType!.GetField("s_resourceManager", BindingFlags.Static | BindingFlags.NonPublic)!;

        return new(resourceManagerField, typeof(System.ComponentModel.Annotations.Strings));
    }

    private SystemResourceLocalizer(
        FieldInfo resourceManagerField,
        Type defaultResourceSourceOverride)
    {
        this.resourceManagerField = resourceManagerField;
        this.defaultResourceSource = defaultResourceSourceOverride;
    }

    /// <summary>
    /// Without this code, no matter what CurrentUICulture is set to, you'll always get English in DataAnnotations validation messages.
    /// It useless to address the issue by installing various language packs on you OS. Microsoft didn't bother to implement or publish
    /// System.ComponentModel.Annotations.resources.dll with their runtime.
    /// Here is a discussion https://github.com/dotnet/aspnetcore/issues/4848#issuecomment-1925245847.
    /// We override DataAnnotationsResources to use a ResourceManager that uses language .resources files embedded in this assembly.
    /// </summary>
    public void OverrideResourceManager(Type? resourceSource = null)
    {
        var resourceManager = GetNewResourceManager(resourceSource ?? defaultResourceSource);
        resourceManagerField.SetValue(null, resourceManager);
    }

    private static ResourceManager GetNewResourceManager(Type resourceSource)
    {
        return new ResourceManager($"{resourceSource.Namespace}.Strings", resourceSource.Assembly);
    }
}
