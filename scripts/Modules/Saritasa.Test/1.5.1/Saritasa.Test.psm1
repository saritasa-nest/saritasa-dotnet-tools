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
        # Path to the testing assembly.
        [Parameter(Mandatory = $true, HelpMessage = 'Path to assembly file with tests.')]
        [string] $TestAssembly,
        # Additional parameters to be passed to NUnit console runner.
        [string[]] $Params,
        # Path to the NUnit console runner. If not specified, the cmdlet will try to find it automatically from current script folder's subfolders.
        [string] $NUnitPath
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    # Format and validate params.
    if (!(Test-Path $TestAssembly))
    {
        throw "$TestAssembly does not exist."
    }
    if ($NUnitPath -and !(Test-Path $NUnitPath))
    {
        throw "$NUnitPath does not exist."
    }

    if (!($NUnitPath))
    {
        # Find nunit3-console.exe
        $packagesDirectory = Get-ChildItem -Filter 'packages' -Recurse -Depth 3 |
            Where-Object { $_.PSIsContainer -and (Get-ChildItem $_.FullName 'NUnit.ConsoleRunner.*') } |
            Select-Object -First 1

        if (!$packagesDirectory)
        {
            throw "Cannot find packages directory. Make sure NUnit.ConsoleRunner package is installed."
        }
        Write-Information "Found $packagesDirectory."
        $nunitExeDirectory = Get-ChildItem $packagesDirectory.FullName 'NUnit.ConsoleRunner.*' |
            Sort-Object { $_.Name } | Select-Object -Last 1
        if (!$nunitExeDirectory)
        {
            throw 'Cannot find nunit console runner package.'
        }
        $NUnitPath = Join-Path $nunitExeDirectory.FullName '.\tools\nunit3-console.exe'
        Write-Information "Found $($nunitExeDirectory.FullName)"
    }

    # Run nunit.
    $args = @($TestAssembly, '--noresult', '--stoponerror', '--noheader')
    $args += $Params
    &"$NUnitPath" $args
    if ($LASTEXITCODE)
    {
        throw "Unit tests failed."
    }
}

<#
.SYNOPSIS
Run xUnit tests since version 2.3 .

.NOTES
xunit.runner.console package should be installed.
#>
function Invoke-XunitRunner
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Path to assembly file with tests.')]
        [string] $TestAssembly,

        [Parameter(HelpMessage = 'Use x86 version of runner. False by default.')]
        [switch] $UseX86,

        [string[]] $Params
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    # Format and validate params.
    if (!(Test-Path $TestAssembly))
    {
        throw "$TestAssembly does not exist."
    }

    # Find xunit.console.exe .
    $packagesDirectory = Get-ChildItem -Filter 'packages' -Recurse -Depth 3 |
        Where-Object { $_.PSIsContainer -and (Get-ChildItem $_.FullName 'xunit.runner.console.*') } |
        Select-Object -First 1

    if (!$packagesDirectory)
    {
        throw "Cannot find packages directory. Make sure xunit.runner.console package is installed."
    }
    Write-Information "Found $packagesDirectory."
    $xunitExeDirectory = Get-ChildItem $packagesDirectory.FullName 'xunit.runner.console.*' |
        Sort-Object { $_.Name } | Select-Object -Last 1
    if (!$xunitExeDirectory)
    {
        throw 'Cannot find xUnit console runner package.'
    }
    $xunitExeFilename = 'xunit.console.exe';
    if ($UseX86)
    {
        $xunitExeFilename = 'xunit.console.x86.exe';
    }
    $xunitExe = Join-Path $xunitExeDirectory.FullName ".\tools\net452\$xunitExeFilename"
    Write-Information "Found $($xunitExeDirectory.FullName)"

    # Run xUnit
    $args = @($TestAssembly, '-nologo', '-nocolor')
    $args += $Params
    &"$xunitExe" $args
    if ($LASTEXITCODE)
    {
        throw "Unit tests failed."
    }
}
