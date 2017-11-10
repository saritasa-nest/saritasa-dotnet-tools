<#PSScriptInfo

.VERSION 1.0.0

.GUID a55519ec-c877-4480-8496-6c87ca097332

.AUTHOR Anton Zimin

.COMPANYNAME Saritasa

.COPYRIGHT (c) 2016 Saritasa. All rights reserved.

.TAGS Psake

.LICENSEURI https://raw.githubusercontent.com/Saritasa/PSGallery/master/LICENSE

.PROJECTURI https://github.com/Saritasa/PSGallery

.ICONURI

.EXTERNALMODULEDEPENDENCIES

.REQUIREDSCRIPTS

.EXTERNALSCRIPTDEPENDENCIES

.RELEASENOTES

.SYNOPSIS
The script is intended for dot-sourcing into default.ps1 (main file for Psake tasks). It allows to override Psake properties from another file.

.DESCRIPTION
Dot source the script:
. .\Saritasa.PsakeExtensions.ps1
#>

<#
.SYNOPSIS
Checks file existence and dot-sources it.
.DESCRIPTION
The file should contain Expand-PsakeConfiguration call.
Call Import-PsakeConfigurationFile from TaskSetup block.
.EXAMPLE
# default.ps1
TaskSetup `
{
    Import-PsakeConfigurationFile ".\Config.$configuration.ps1"
}

# Config.Debug.ps1

Expand-PsakeConfiguration `
@{
    serverHost = 'dev.example.com'
}
#>
function Import-PsakeConfigurationFile
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $fileName = Resolve-Path $Path
    if (Test-Path $fileName)
    {
        . $fileName
    }
}

<#
.SYNOPSIS
Extends Psake properties from the hashtable.
#>
function Expand-PsakeConfiguration
{
    param
    (
        [Parameter(Mandatory = $true)]
        [hashtable]
        $NewConfiguration
    )

    for ($i = 1; $i -lt 20; $i++)
    {
        try
        {
            Get-Variable -Name 'properties' -Scope $i -ErrorAction SilentlyContinue | Out-Null
        }
        catch
        {
            break
        }
    }
    $maxScope = $i - 1
    $cmdLineScope = $maxScope - 2
    $propertiesScope = $cmdLineScope - 3
    
    # Override properties from passed hashtable.
    foreach ($key in $NewConfiguration.Keys)
    {
        Write-Debug "Set1: $key = $($NewConfiguration.$key)"
        Set-Variable -Name $key -Value $NewConfiguration.$key -Scope $propertiesScope | Out-Null
    }

    # Override properties from command line arguments.
    $properties = (Get-Variable 'properties' -Scope $cmdLineScope).Value
    foreach ($key in $properties.keys)
    {
        Write-Debug "Set2: $key"
        if (Test-Path "variable:\$key")
        {
            Write-Debug "Set3: $key = $($properties.$key)"
            Set-Variable -Name $key -Value $properties.$key -Scope $propertiesScope | Out-Null
        }
    }
}
