$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"
$tools = "$root\..\tools"

Task download-nuget `
{
    if (!(Test-Path $tools))
    {
        New-Item -ItemType Directory $tools
    }

    Install-NugetCli -Destination $tools
}

Task pre-build -depends download-nuget `
{
    Invoke-NugetRestore "$src\..\Saritasa.Tools.sln"

    Invoke-NugetRestore "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.sln"
    Invoke-NugetRestore "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.sln"

    GitVersion.exe /updateassemblyinfo
}

Task get-version `
{
    $script:Version = GitVersion.exe /showvariable MajorMinorPatch
}

Task build-samples -depends build-zergrushco, build-boringwarehouse

Task build-zergrushco -depends pre-build `
{
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

Task build-boringwarehouse -depends pre-build `
{
    Invoke-SolutionBuild "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.sln" -Configuration $Configuration
}

Task build-saritasatools -depends pre-build `
{
    Invoke-SolutionBuild "$src\Saritasa.Tools.sln" -Configuration $Configuration
}
