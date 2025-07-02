using System.Reflection;
using System.Resources;

namespace Saritasa.Tools.Domain.Localization;

/// <summary>
/// Localization provider for non-localized system resources.
/// See also <seealso href="https://github.com/saritasa-nest/saritasa-dotnet-tools/wiki/Domain/Domain-Localization"/>.
/// </summary>
public static class SystemResourceLocalizer
{
    /// <summary>
    /// Override System.ComponentModel.Annotations to use alternative ResourceManager.
    /// Replace embedded .resources.dll file with the custom resource provider.
    /// </summary>
    /// <remarks>
    /// Without this code, no matter what CurrentUICulture is set to, you'll always get English in DataAnnotations validation messages.
    /// <para>
    /// It useless to address the issue by installing various language packs on you OS.
    /// Microsoft does no longer publish localized System.ComponentModel.Annotations.resources.dll with CoreCLR runtime.
    /// </para>
    /// <para>
    /// Link to a discussion <see href="https://github.com/dotnet/aspnetcore/issues/4848#issuecomment-1925245847."/>.
    /// </para>
    /// </remarks>
    /// <param name="resourceSource">
    /// Resource provider type.
    /// Default value is provided by Saritasa.Tools.Domain.resources.dll.
    /// </param>
    public static void OverrideComponentModelAnnotations(Type? resourceSource = null)
    {
        // Ensure overridable assembly is loaded.
        var test = typeof(global::System.ComponentModel.DataAnnotations.RequiredAttribute);

        // Get overridable resource source.
        var srType = test.Assembly.GetType("System.SR");

        // Get overridable resource manager field setter.
        var resourceManagerField = srType!.GetField("s_resourceManager", BindingFlags.Static | BindingFlags.NonPublic)!;

        var resourceManager = new ResourceManager(resourceSource ?? typeof(System.ComponentModel.Annotations.Strings));
        resourceManagerField.SetValue(null, resourceManager);
    }
}
