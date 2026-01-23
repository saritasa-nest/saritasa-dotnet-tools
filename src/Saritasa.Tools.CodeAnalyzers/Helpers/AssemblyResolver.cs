using Saritasa.Tools.CodeAnalyzers.Analyzers;

namespace Saritasa.Tools.CodeAnalyzers.Helpers;

/// <summary>
/// Assembly resolver for code analyzers to resolve dependencies at runtime. By default dependencies' assemblies are not loaded.
/// </summary>
public static class AssemblyResolver
{
    private static readonly object AssemblyResolverLock = new();
    private static bool assemblyResolverInstalled;

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
    /// <summary>
    /// Resolved assemblies specified in <see cref="IsKnownDependency"/> method.
    /// </summary>
    public static void ResolveAssemblies()
    {
        lock (AssemblyResolverLock)
        {
            if (assemblyResolverInstalled)
            {
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                try
                {
                    var requested = new System.Reflection.AssemblyName(args.Name);
                    var requestedName = requested.Name;
                    if (string.IsNullOrWhiteSpace(requestedName) || !IsKnownDependency(requestedName))
                    {
                        return null;
                    }

                    var analyzerLocation = typeof(SpellingAnalyzer).Assembly.Location;
                    var analyzerDir = Path.GetDirectoryName(analyzerLocation);
                    if (string.IsNullOrWhiteSpace(analyzerDir))
                    {
                        return null;
                    }

                    var fileName = requestedName + ".dll";

                    var candidate = Path.Combine(analyzerDir, fileName);
                    if (File.Exists(candidate))
                    {
                        return System.Reflection.Assembly.LoadFrom(candidate);
                    }

                    var analyzersDirCandidate = Path.Combine(analyzerDir, "analyzers", fileName);
                    if (File.Exists(analyzersDirCandidate))
                    {
                        return System.Reflection.Assembly.LoadFrom(analyzersDirCandidate);
                    }
                }
                catch
                {
                    // Ignore and let default resolution continue.
                }

                return null;
            };

            assemblyResolverInstalled = true;
        }
    }
#pragma warning restore RS1035

    private static bool IsKnownDependency(string assemblyName) => assemblyName switch
    {
        "WeCantSpell.Hunspell" => true,
        _ => false,
    };
}
