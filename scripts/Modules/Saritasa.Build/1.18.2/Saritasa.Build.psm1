$assemblyVersionRegex = $null
$assemblyInfoVersionRegex = $null
$assemblyFileVersionRegex = $null

<#
.SYNOPSIS
Downloads latest nuget.exe to specified location.

.EXAMPLE
Install-NugetCli .

Install nuget into current directory
#>
function Install-NugetCli
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Destination
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    $nugetExePath = "$Destination\nuget.exe"

    if (!(Test-Path $nugetExePath))
    {
        Write-Information 'Downloading nuget.exe...'
        Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile $nugetExePath
        Write-Information 'Done.'
    }
}

<#
.SYNOPSIS
Restores packages for solution, project or packages.config.

.EXAMPLE
Invoke-NugetRestore .\..\myapp.sln

Restores all packages for myapp solution.

.NOTES
If nuget command is not found - it will be downloaded to current directory.
#>
function Invoke-NugetRestore
{
    [CmdletBinding()]
    param
    (
        # Path to solution. All NuGet packages from included projects will be restored.
        [Parameter(Mandatory = $true, ParameterSetName = 'Solution')]
        [string] $SolutionPath,
        # Path to project or packages.config.
        [Parameter(Mandatory = $true, ParameterSetName = 'Project')]
        [string] $ProjectPath,
        # Path to the solution directory. Not valid when restoring packages for a solution.
        [Parameter(Mandatory = $true, ParameterSetName = 'Project')]
        [string] $SolutionDirectory
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    $nugetCli = Get-Command nuget.exe -ErrorAction SilentlyContinue

    if ($nugetCli)
    {
        $nugetExePath = $nugetCli.Source
    }
    else
    {
        Write-Warning "Install NuGet globally for faster builds:`nchoco install nuget.commandline"
        
        Install-NugetCli -Destination $PSScriptRoot
        $nugetExePath = "$PSScriptRoot\nuget.exe"
    }

    $params = @('restore')
    if ($SolutionPath)
    {
        $params += $SolutionPath
    }
    else
    {
        $params += @($ProjectPath, '-SolutionDirectory', $SolutionDirectory)
    }

    &$nugetExePath $params
    if ($LASTEXITCODE)
    {
        throw 'Nuget restore failed.'
    }
}

<#
.SYNOPSIS
Builds solution.

.EXAMPLE
Invoke-SolutionBuild .\..\myapp.sln -Configuration Debug
#>
function Invoke-SolutionBuild
{
    [CmdletBinding()]
    param
    (
        # Path to solution.
        [Parameter(Mandatory = $true)]
        [string] $SolutionPath,
        # Build configuration (Release, Debug, etc.)
        [string] $Configuration
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    Invoke-ProjectBuild $SolutionPath $Configuration
}

<#
.SYNOPSIS
Builds project.

.PARAMETER Target
Build the specified targets in the project.
Use a semicolon or comma to separate multiple targets.

.EXAMPLE
Invoke-ProjectBuild .\..\Web\Web.csproj -Configuration 'Release'

.NOTES
For more information about Target and BuildParams parameters, see MSBuild documentation.
#>
function Invoke-ProjectBuild
{
    [CmdletBinding()]
    param
    (
        # Path to project.
        [Parameter(Mandatory = $true)]
        [string] $ProjectPath,
        # Build configuration (Release, Debug, etc.)
        [string] $Configuration,
        [string] $Target = 'Build',
        # Additional build parameters.
        [string[]] $BuildParams
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    msbuild.exe $ProjectPath '/m' "/t:$Target" "/p:Configuration=$Configuration" '/verbosity:normal' $BuildParams
    if ($LASTEXITCODE)
    {
        throw 'Build failed.'
    }
}

function ReplaceOrAppend([regex] $ReplaceRegex, [string] $InputString, [string] $ReplaceString, [string] $AssemblyAddFormat)
{
    if ($ReplaceRegex.IsMatch($InputString))
    {
        $InputString = $ReplaceRegex.Replace($InputString, $ReplaceString)
    }
    else
    {
        $InputString += [Environment]::NewLine + [string]::Format($AssemblyAddFormat, $ReplaceString)
    }

    $InputString
}

function ProcessAssemblyInfoFile([string] $FileName, [string] $AssemblyVersionString, [string] $AssemblyInfoVersionString, [string] $AssemblyFileVersionString)
{
    $assemblyAddFormat = switch ((Get-Item $FileName).Extension)
        {
            '.cs' { '[assembly: {0}]' }
            '.vb' { '<Assembly: {0}>' }
            '.fs' { '[<assembly: {0}>]' }
            default { $null }
        }

    if (!$assemblyAddFormat)
    {
        Write-Warning "Unknown file format: $FileName"
    }

    $content = Get-Content $FileName -Raw -Encoding UTF8

    if ($AssemblyVersionString)
    {
        $content = ReplaceOrAppend $script:assemblyVersionRegex $content $AssemblyVersionString $assemblyAddFormat
    }

    if ($AssemblyFileVersionString)
    {
        $content = ReplaceOrAppend $script:assemblyFileVersionRegex $content $AssemblyFileVersionString $assemblyAddFormat
    }

    if ($AssemblyInfoVersionString)
    {
        $content = ReplaceOrAppend $script:assemblyInfoVersionRegex $content $AssemblyInfoVersionString $assemblyAddFormat
    }

    Set-Content -Path $FileName -Encoding UTF8 -Value $content
}

<#
.SYNOPSIS
Update version numbers of AssemblyInfo.cs and AssemblyInfo.vb.

.DESCRIPTION
Updates version numbers in AssemblyInfo files located in current directory and all subdirectories.

.EXAMPLE
Update-AssemblyInfoFile -AssemblyVersion '6.3.0.0' -AssemblyFileVersion '6.3.1.1' -AssemblyInfoVersion '6.3.0+master.808d1733f5c701c1882816f70c3eafc6e7fce4d4'

.NOTES
Based on SetVersion script.
http://www.luisrocha.net/2009/11/setting-assembly-version-with-windows.html
Copyright (c) 2009 Luis Rocha

Based on GitVersion code.
https://github.com/GitTools/GitVersion
#>
function Update-AssemblyInfoFile
{
    [CmdletBinding(SupportsShouldProcess = $true)]
    param
    (
        # Version string in major.minor.build.revision format.
        [Parameter(Mandatory = $true)]
        [Alias("Version")]
        [string] $AssemblyVersion,
        [string] $AssemblyInfoVersion,
        [string] $AssemblyFileVersion,
        # Path to root directory.
        [string] $Path
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    $script:assemblyVersionRegex = [regex] 'AssemblyVersion(Attribute)?\s*\(.*\)\s*'
    $assemblyVersionString = if ($AssemblyVersion) { "AssemblyVersion(`"$AssemblyVersion`")" }

    $script:assemblyInfoVersionRegex = [regex] 'AssemblyInformationalVersion(Attribute)?\s*\(.*\)\s*'
    $assemblyInfoVersionString = if ($AssemblyInfoVersion) { "AssemblyInformationalVersion(`"$AssemblyInfoVersion`")" }

    $script:assemblyFileVersionRegex = [regex] 'AssemblyFileVersion(Attribute)?\s*\(.*\)\s*'
    $assemblyFileVersionString = if ($AssemblyFileVersion) { "AssemblyFileVersion(`"$AssemblyFileVersion`")" }

    $rootDirectory = if ($Path) { $Path } else { Get-Location }

    Get-ChildItem -Path $rootDirectory -Recurse -Include AssemblyInfo.cs, AssemblyInfo.vb, AssemblyInfo.fs | ForEach-Object `
        {
            $fileName = $_.FullName

            if ($PSCmdlet.ShouldProcess($fileName))
            {
                ProcessAssemblyInfoFile $fileName $assemblyVersionString $assemblyInfoVersionString $assemblyFileVersionString
                Write-Information "$fileName -> $AssemblyVersion"
            }
        }
}

<#
.SYNOPSIS
Creates file from a template.

.DESCRIPTION
Creates a config file from it's template. If file already exists, it will not be overridden.

.EXAMPLE
Copy-DotnetConfig .\..\Web\Web.config.template

Creates a Web.config file in Web folder from template.
#>
function Copy-DotnetConfig
{
    [CmdletBinding()]
    param
    (
        # Path to App.config.template or Web.config.template file.
        [Parameter(Mandatory = $true)]
        [string] $TemplateFilename
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    $configFilename = $TemplateFilename -replace '\.template', ''
    if (!(Test-Path $configFilename))
    {
        Copy-Item $TemplateFilename $configFilename
    }
}

<#
.SYNOPSIS
Run Entity Framework migrations.

.EXAMPLE
Invoke-EFMigrate ..\..\Domain\bin\Debug\Domain.dll

Runs all migrations declared in Domain.dll file, using Domain.dll.config as configuration file

.NOTES
In essential this command tries to find migrate.exe in packages and run it against specified
configuration file.
#>
function Invoke-EFMigrate
{
    [CmdletBinding()]
    param
    (
        # Path to assembly file with migrations.
        [Parameter(Mandatory = $true)]
        [string] $MigrationAssembly,
        # Path to assembly .config file. If not specified default or parent Web.config will be used.
        [string] $ConfigFilename
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    # Format and validate params
    if (!$ConfigFilename)
    {
        $ConfigFilename = $MigrationAssembly + '.config'
        if (!(Test-Path $ConfigFilename))
        {
            $ConfigFilename = Join-Path (Split-Path $MigrationAssembly) '..\Web.config'
        }
    }
    if (!(Test-Path $ConfigFilename))
    {
        throw "$ConfigFilename does not exist."
    }
    if (!(Test-Path $MigrationAssembly))
    {
        throw "$MigrationAssembly does not exist."
    }

    # Find migrate.exe
    $packagesDirectory = Get-ChildItem 'packages' -Recurse -Depth 3 |
        Where-Object { $_.PSIsContainer } | Select-Object -First 1
    if (!$packagesDirectory)
    {
        throw 'Cannot find packages directory.'
    }
    Write-Information "Found $($packagesDirectory.FullName)"
    $migrateExeDirectory = Get-ChildItem $packagesDirectory.FullName 'EntityFramework.*' |
        Sort-Object { $_.Name } | Select-Object -Last 1
    if (!$migrateExeDirectory)
    {
        throw 'Cannot find entity framework package.'
    }
    $migrateExe = Join-Path $migrateExeDirectory.FullName '.\tools\migrate.exe'
    Write-Information "Found $($migrateExeDirectory.FullName)"

    # Run migrate
    $workingDirectory = Get-Location
    $args = @(
        [System.IO.Path]::GetFileName($MigrationAssembly)
        '/startUpDirectory:"{0}"' -f (Join-Path $workingDirectory (Split-Path $MigrationAssembly))
        '/startUpConfigurationFile:"{0}"' -f (Join-Path $workingDirectory $ConfigFilename)
    );
    &"$migrateExe" $args
    if ($LASTEXITCODE)
    {
        throw "Migration failed."
    }
}

<#
.SYNOPSIS
Replaces placeholders $(UserName) with values from hashtable.

.EXAMPLE
Update-VariablesInFile -Path Config.xml @{UserName='sa'}
#>
function Update-VariablesInFile
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Path,
        [Parameter(Mandatory = $true)]
        [hashtable] $Variables
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    $content = Get-Content $Path

    foreach ($key in $Variables.Keys)
    {
        $escapedValue = $Variables[$key] -replace '\$', '$$$$'
        $content = $content -ireplace"\`$\($key\)", $escapedValue
    }

    $content | Set-Content $Path
}

<#
.SYNOPSIS
Adds correct path to MSBuild to Path environment variable.
#>
function Initialize-MSBuild
{
    [CmdletBinding()]
    param ()

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    $vsPath = (@((Get-VSSetupInstance | Select-VSSetupInstance -Version 15.0 -Require Microsoft.Component.MSBuild).InstallationPath,
        (Get-VSSetupInstance | Select-VSSetupInstance -Version 15.0 -Product Microsoft.VisualStudio.Product.BuildTools).InstallationPath) -ne $null)[0]

    if (!$vsPath)
    {
        Write-Information 'VS 2017 not found.'
        return
    }

    if ([System.IntPtr]::Size -eq 8)
    {
        $msbuildPath = Join-Path $vsPath 'MSBuild\15.0\Bin\amd64'
    }
    else
    {
        $msbuildPath = Join-Path $vsPath 'MSBuild\15.0\Bin'
    }

    $env:Path = "$msbuildPath;$env:Path"
}

<#
.SYNOPSIS
Loads packages from multiple packages.config and saves to a single file.

.EXAMPLE
Merge-PackageConfigs -SolutionDirectory .\src -OutputPath .\src\packages.merged.config

.EXAMPLE
Merge-PackageConfigs -SolutionDirectory .\src -OutputPath .\src\packages.merged.net40.config -Framework net40
Merge-PackageConfigs -SolutionDirectory .\src -OutputPath .\src\packages.merged.net452.config -Framework net452
#>
function Merge-PackageConfigs
{
    [CmdletBinding()]
    param
    (
        # Directory in which to look for packages.config files.
        [Parameter(Mandatory = $true)]
        [string] $SolutionDirectory,
        # Path to file in which results should be saved. If file exists, it will be overridden.
        [Parameter(Mandatory = $true)]
        [string] $OutputPath,
        # If specified, only packages with this framework will be included in the results.
        [string] $Framework
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    $files = Get-ChildItem $SolutionDirectory -Recurse packages.config

    $packagesSet = New-Object 'System.Collections.Generic.HashSet[string]'

    foreach ($file in $files)
    {
        [xml] $xml = Get-Content $file.FullName
        $xml.packages.package | ForEach-Object `
            {
                if (!$Framework -or $_.targetFramework -eq $Framework)
                {
                    $packagesSet.Add($_.OuterXml) | Out-Null
                }
            }
    }

    [xml] $finalXml = '<?xml version="1.0" encoding="utf-8"?><packages>' + $packagesSet + '</packages>'
    $finalXml.Save($OutputPath)
}

<#
.SYNOPSIS
Removes Roslyn analyzer references from Visual Studio project.
#>
function Remove-RoslynAnalyzer
{
    [CmdletBinding()]
    param
    (
        # Path to Visual Studio project.
        [Parameter(Mandatory = $true)]
        [string] $ProjectPath
    )

    $xml = [xml](Get-Content $ProjectPath)
    $project = $xml.DocumentElement

    $nodesToRemove = @()
    $xml.GetElementsByTagName("ItemGroup") | ForEach-Object `
        {
            if ($_.GetElementsByTagName('Analyzer').Count)
            {
                $nodesToRemove += $_
            }
        }

    $nodesToRemove | ForEach-Object { $project.RemoveChild($_) } | Out-Null

    $xml.Save($ProjectPath)
}
