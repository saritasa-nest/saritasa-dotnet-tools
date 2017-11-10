<#
.SYNOPSIS
Downloads nuget.exe to specified location.
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
#>
function Invoke-NugetRestore
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Path to solution. All NuGet packages from included projects will be restored.', ParameterSetName = 'Solution')]
        [string] $SolutionPath,
        [Parameter(Mandatory = $true, HelpMessage = 'Path to project or packages.config.', ParameterSetName = 'Project')]
        [string] $ProjectPath,
        [Parameter(Mandatory = $true, HelpMessage = 'Path to the solution directory. Not valid when restoring packages for a solution.', ParameterSetName = 'Project')]
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

function Invoke-SolutionBuild
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Path to solution.')]
        [string] $SolutionPath,
        [Parameter(HelpMessage = 'Build configuration (Release, Debug, etc.)')]
        [string] $Configuration
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    Invoke-ProjectBuild $SolutionPath $Configuration
}

function Invoke-ProjectBuild
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Path to project.')]
        [string] $ProjectPath,
        [Parameter(HelpMessage = 'Build configuration (Release, Debug, etc.)')]
        [string] $Configuration,
        [string] $Target = 'Build',
        [string[]] $BuildParams
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    msbuild.exe $ProjectPath '/m' "/t:$Target" "/p:Configuration=$Configuration" '/verbosity:normal' $BuildParams
    if ($LASTEXITCODE)
    {
        throw 'Build failed.'
    }
}

<#
.SYNOPSIS
Update version numbers of AssemblyInfo.cs and AssemblyInfo.vb.

.NOTES
Based on SetVersion script.
http://www.luisrocha.net/2009/11/setting-assembly-version-with-windows.html
Copyright (c) 2009 Luis Rocha
#>
function Update-AssemblyInfoFile
{
    [CmdletBinding(SupportsShouldProcess = $true)]
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Version string in major.minor.build.revision format.')]
        [string] $Version
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $assemblyVersion = 'AssemblyVersion("' + $Version + '")';
    $fileVersion = 'AssemblyFileVersion("' + $Version + '")';

    Get-ChildItem -r -Include AssemblyInfo.cs, AssemblyInfo.vb | ForEach-Object `
        {
            $filename = $_.Directory.ToString() + '\' + $_.Name

            # If you are using a source control that requires to check-out files before
            # modifying them, make sure to check-out the file here.
            # For example, TFS will require the following command:
            # tf checkout $filename

            if ($PSCmdlet.ShouldProcess($filename))
            {
                (Get-Content $filename) | ForEach-Object `
                    {
                        ForEach-Object { $_ -replace $assemblyVersionPattern, $assemblyVersion } |
                        ForEach-Object { $_ -replace $fileVersionPattern, $fileVersion }
                    } | Set-Content $filename -Encoding UTF8

                Write-Information ($filename + ' -> ' + $Version)
            }
        }
}

function Copy-DotnetConfig
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Path to App.config.template or Web.config.template file.')]
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

.NOTES
In essential this command tries to find migrate.exe in packages and run it against specified
configuration file.
#>
function Invoke-EFMigrate
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Path to assembly file with migrations.')]
        [string] $MigrationAssembly,
        [Parameter(HelpMessage = 'Path to assembly .config file. If not specified default or parent Web.config will be used.')]
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
    Write-Information "Found $packagesDirectory.FullName"
    $migrateExeDirectory = Get-ChildItem $packagesDirectory.FullName 'EntityFramework.*' |
        Sort-Object { $_.Name } | Select-Object -Last 1
    if (!$migrateExeDirectory)
    {
        throw 'Cannot find entity framework package.'
    }
    $migrateExe = Join-Path $migrateExeDirectory.FullName '.\tools\migrate.exe'
    Write-Information "Found $migrateExeDirectory.FullName"

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

    $env:Path = $msbuildPath + ";$env:Path"
}

<#
.SYNOPSIS
Loads packages from many packages.config and saves to a single file.
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
        [Parameter(Mandatory = $true)]
        [string] $SolutionDirectory,
        [Parameter(Mandatory = $true)]
        [string] $OutputPath,
        [Parameter]
        [string] $Framework
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    $files = Get-ChildItem $src -Recurse packages.config

    $packagesSet = New-Object 'System.Collections.Generic.HashSet[string]'

    foreach ($file in $files)
    {
        [xml] $xml = Get-Content $file.FullName
        $xml.packages.package | % `
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
