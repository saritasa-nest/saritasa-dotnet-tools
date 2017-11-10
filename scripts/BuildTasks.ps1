Properties `
{
    $Configuration = $null
}

$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"
$tools = "$root\..\tools"

Task pre-build `
{
    Initialize-MSBuild
    Invoke-NugetRestore -SolutionPath "$src\..\Saritasa.Tools.sln"

    Invoke-NugetRestore -SolutionPath "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.sln"
    Invoke-NugetRestore -SolutionPath "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.sln"

    # Use following command to revert the files:
    # git checkout -- **/AssemblyInfo.cs
    Exec { GitVersion.exe /updateassemblyinfo }
}

Task build-samples -depends build-zergrushco, build-boringwarehouse

Task build-zergrushco -depends pre-build `
    -requiredVariables @('Configuration') `
{
    Invoke-SolutionBuild "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.sln" -Configuration $Configuration

    Set-Location "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web"
    Exec { npm i }
    Exec { bower i }

    if (!(Test-Path 'Static'))
    {
        New-Item 'Static' -ItemType Directory
    }

    $gulpArgument = ''
    if ($Configuration -eq 'Release')
    {
        $gulpArgument = '--production'
    }

    Exec { gulp $gulpArgument }

    Set-Location "$samples\.."
}

Task build-boringwarehouse -depends pre-build `
    -requiredVariables @('Configuration') `
{
    Invoke-SolutionBuild "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.sln" -Configuration $Configuration
}

Task build-saritasatools -depends pre-build `
    -requiredVariables @('Configuration') `
{
    Invoke-SolutionBuild "$src\..\Saritasa.Tools.sln" -Configuration $Configuration
}

Task clean -description '* Clean repository.' `
{
    Exec { git clean -xdf -e packages/ -e node_modules/ -e bower_components/ -e nuget.exe }
}
