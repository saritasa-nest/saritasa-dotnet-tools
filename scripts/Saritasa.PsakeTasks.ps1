#Requires -Modules psake

<#PSScriptInfo

.VERSION 1.3.1

.GUID 966fce03-6946-447c-8e16-29b673f2918b

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
Contains common Psake tasks.

.DESCRIPTION

#>

Properties `
{
    $ScriptsPath = $null
}

# Tasks with description starting with * are main. They are shown on the help screen. Other tasks are auxiliary.
Task help -description 'Display description of main tasks.' `
{
    Write-Host 'Main Tasks' -ForegroundColor DarkMagenta -NoNewline
    Get-PSakeScriptTasks | Where-Object { $_.Description -Like '`**' } |
        Sort-Object -Property Name |
        Format-Table -Property Name, @{ Label = 'Description'; Expression = { $_.Description -Replace '^\* ', '' } }

    Write-Host 'Execute ' -NoNewline -ForegroundColor DarkMagenta
    Write-Host 'psake -docs' -ForegroundColor Black -BackgroundColor DarkMagenta -NoNewline
    Write-Host ' to see all tasks.' -ForegroundColor DarkMagenta
}

Task default -depends help -description 'Show automatically generated help.'

Task update-gallery -description '* Update all modules from Saritasa PS Gallery.' `
{
    $baseUri = 'https://raw.githubusercontent.com/Saritasa/PSGallery/master'

    if ($ScriptsPath)
    {
        $root = $ScriptsPath
    }
    else
    {
        $root = $PSScriptRoot
    }

    $modules = "$root\Modules"

    # Remove old modules.
    Get-ChildItem -Path $modules -Directory | ForEach-Object `
        {
            Remove-Item "$($_.FullName)\*" -Recurse -ErrorAction Continue -Force
        }

    Get-ChildItem -Path $modules -Directory | ForEach-Object `
        {
            Write-Information "Updating $($_.Name)..."
            Save-Module -Name $_.Name -Path $modules
            Write-Information 'OK'
        }

    Get-ChildItem -Path $root -Include 'Saritasa*Tasks.ps1' -Recurse | ForEach-Object `
        {
            Write-Information "Updating $($_.Name)..."
            Invoke-WebRequest -Uri "$baseUri/scripts/Psake/$($_.Name)" -OutFile "$root\$($_.Name)"
            Write-Information 'OK'
        }

    Invoke-Task add-scripts-to-git
}

Task add-scripts-to-git -description 'Add PowerShell scripts and modules to Git.' `
{
    $root = $PSScriptRoot

    Get-ChildItem -Path $root -File -Recurse -Exclude '*.exe' -Force | ForEach-Object `
        {
            Exec { git add -f $_.FullName }
        }
}
