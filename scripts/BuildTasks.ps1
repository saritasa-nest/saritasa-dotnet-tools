$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"
$tools = "$root\..\tools"

Task pre-build `
{
    Invoke-NugetRestore "$src\Saritasa.Tools.sln"

    if (!(Test-Path $tools))
    {
        New-Item -ItemType Directory $tools
    }

    Install-NugetCli -Destination $tools
    &"$tools\nuget.exe" restore "$src\Saritasa.Tools.NLog4\packages.config" -SolutionDirectory $src

    Invoke-NugetRestore "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.sln"
    Invoke-NugetRestore "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.sln"
}

Task build-samples -depends build-zergrushco, build-boringwarehouse

Task build-zergrushco -depends pre-build `
{
    Invoke-SolutionBuild "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.sln" -Configuration $Configuration

    Set-Location "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web"
    npm i
    bower i

    if (!(Test-Path 'Static'))
    {
        New-Item 'Static' -ItemType Directory
    }

    gulp
}

Task build-boringwarehouse -depends pre-build `
{
    Invoke-SolutionBuild "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.sln" -Configuration $Configuration
}
