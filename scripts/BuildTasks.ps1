$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

Task build-zerg `
{
    Invoke-NugetRestore "$src\Saritasa.Tools.sln"
    Invoke-NugetRestore "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.sln"
    Invoke-SolutionBuild "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.sln" -Configuration $Configuration
}
