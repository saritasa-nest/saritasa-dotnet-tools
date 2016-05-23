<#
.SYNOPSIS
Run NUnit 3 tests.

.NOTES
NUnit.ConsoleRunner package should be installed.
#>
function Invoke-Nunit3Runner
{
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Path to assembly file with tests.')]
        [string] $TestAssembly,
        [string[]] $Params
    )

    # Format and validate params
    if (!(Test-Path $TestAssembly))
    {
        throw "$TestAssembly does not exist."
    }

    # Find nunit3-console.exe
    $packagesDirectory = Get-ChildItem 'packages' -Recurse -Depth 3 | ? {$_.PSIsContainer} | Select -First 1
    if (!$packagesDirectory)
    {
        throw 'Cannot find packages directory.'
    }
    Write-Information "Found $packagesDirectory.FullName"
    $nunitExeDirectory = Get-ChildItem $packagesDirectory.FullName 'NUnit.ConsoleRunner.*' | Sort-Object {$_.Name} | Select -Last 1
    if (!$nunitExeDirectory)
    {
        throw 'Cannot find nunit console runner package.'
    }
    $nunitExe = Join-Path $nunitExeDirectory.FullName '.\tools\nunit3-console.exe'
    Write-Information "Found $nunitExeDirectory.FullName"

    # Run nunit
    $args = @($TestAssembly, '--noresult', '--stoponerror', '--noheader')
    $args += $Params
    &"$nunitExe" $args
    if ($LASTEXITCODE)
    {
        throw "Unit tests failed."
    }
}
