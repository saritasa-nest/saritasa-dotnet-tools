$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

Task build-zerg `
{
    Invoke-NugetRestore "$src\Saritasa.Tools.sln"
    Invoke-NugetRestore "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.sln"
    Invoke-SolutionBuild "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.sln" -Configuration $Configuration

    Set-Location "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web"
    Exec { npm i }
    Exec { bower i }

    if (!(Test-Path 'Static'))
    {
        New-Item 'Static' -ItemType Directory
    }

    Exec { gulp }

    Set-Location "$samples\.."
}

Task build-bw `
{
    Invoke-NugetRestore "$src\Saritasa.Tools.sln"
    &"$root\nuget.exe" restore "$src\Saritasa.Tools.NLog4\packages.config" -SolutionDirectory $src
    Invoke-NugetRestore "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.sln"
    Invoke-SolutionBuild "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.sln" -Configuration $Configuration
}
