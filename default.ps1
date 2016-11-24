# Requires psake to run, see README.md for more details.

if ($PSVersionTable.PSVersion.Major -lt 3)
{
    throw "PowerShell 3 is required.`nhttp://www.microsoft.com/en-us/download/details.aspx?id=40855"
}

Framework 4.6
$InformationPreference = 'Continue'
$env:PSModulePath += ";$PSScriptRoot\scripts\Modules"

. .\scripts\Saritasa.PsakeTasks.ps1

. .\scripts\BuildTasks.ps1
. .\scripts\PublishTasks.ps1

Import-Module Saritasa.Build
Import-Module Saritasa.Test

# Global variable.
$script:Version = '0.0.0'

$packages = @(
    'Saritasa.Tools.Common'
    'Saritasa.Tools.Domain'
    'Saritasa.Tools.Ef6'
    'Saritasa.Tools.EfCore1'
    'Saritasa.Tools.Emails'
    'Saritasa.Tools.Messages'
    'Saritasa.Tools.Misc'
    'Saritasa.Tools.Mvc5'
    'Saritasa.Tools.NLog4'
)

$docsRoot = Resolve-Path "$PSScriptRoot\docs"

function Get-PackageName([string]$package)
{
    If ([System.Char]::IsDigit($package[$package.Length-1])) {$package.Substring(0, $package.Length-1)} Else {$package}
}

Task pack -description 'Build the library, test it and prepare nuget packages' `
{
    foreach ($package in $packages)
    {
        &dotnet pack ".\src\$package" --configuration release --output '.'
        if ($LASTEXITCODE)
        {
            throw 'Nuget pack failed.'
        }
    }
}

Task clean -description 'Clean solution' `
{
    Remove-Item $LibDirectory -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item './Saritasa.*.nupkg' -ErrorAction SilentlyContinue
    Remove-Item './src/*.suo' -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item './src/Saritasa.Tools/bin' -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item './src/Saritasa.Tools/obj' -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item './src/StyleCop.Cache' -Force -ErrorAction SilentlyContinue
    Remove-Item './docs/_build' -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item './docs/conf.py' -ErrorAction SilentlyContinue
    Remove-Item './scripts/nuget.exe' -ErrorAction SilentlyContinue
}

Task docs -depends get-version -description 'Compile and open documentation' `
{
    CompileDocs
    Invoke-Item './docs/_build/html/index.html'
}

function CompileDocs
{
    Copy-Item "$docsRoot\conf.py.template" "$docsRoot\conf.py"
    (Get-Content "$docsRoot\conf.py").Replace('VX.VY', $Version) | Set-Content "$docsRoot\conf.py"
    
    python -m sphinx.__init__ -b html -d "$docsRoot\_build\doctrees" $docsRoot "$docsRoot\_build\html"
    if ($LASTEXITCODE)
    {
        throw 'Cannot compile documentation.'
    }
}
