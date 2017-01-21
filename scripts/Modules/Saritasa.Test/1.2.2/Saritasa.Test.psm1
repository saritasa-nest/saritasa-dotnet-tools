<#
.SYNOPSIS
Run NUnit 3 tests.

.NOTES
NUnit.ConsoleRunner package should be installed.
#>
function Invoke-Nunit3Runner
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Path to assembly file with tests.')]
        [string] $TestAssembly,
        [string[]] $Params
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    # Format and validate params
    if (!(Test-Path $TestAssembly))
    {
        throw "$TestAssembly does not exist."
    }

    # Find nunit3-console.exe
    $packagesList = Get-ChildItem -Filter 'packages' -Recurse -Depth 3 | Where-Object { $_.PSIsContainer }
    foreach ($pd in $packagesList)
    {
        $nunitDir = Get-ChildItem $pd.FullName 'NUnit.ConsoleRunner.*'
        if ($nunitDir)
        {
            $packagesDirectory = $pd.FullName
            break
        }
    }

    if (!$packagesDirectory)
    {
        throw 'Cannot find packages directory.'
    }
    Write-Information "Found $packagesDirectory."
    $nunitExeDirectory = Get-ChildItem $packagesDirectory 'NUnit.ConsoleRunner.*' |
        Sort-Object { $_.Name } | Select-Object -Last 1
    if (!$nunitExeDirectory)
    {
        throw 'Cannot find nunit console runner package.'
    }
    $nunitExe = Join-Path $nunitExeDirectory.FullName '.\tools\nunit3-console.exe'
    Write-Information "Found $($nunitExeDirectory.FullName)"

    # Run nunit
    $args = @($TestAssembly, '--noresult', '--stoponerror', '--noheader')
    $args += $Params
    &"$nunitExe" $args
    if ($LASTEXITCODE)
    {
        throw "Unit tests failed."
    }
}
