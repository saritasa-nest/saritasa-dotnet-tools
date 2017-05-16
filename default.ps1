# Requires psake to run, see README.md for more details.

Framework 4.6
$InformationPreference = 'Continue'
$env:PSModulePath += ";$PSScriptRoot\Scripts\Modules"

if ($PSVersionTable.PSVersion.Major -lt 3)
{
    throw "PowerShell 3 is required.`nhttp://www.microsoft.com/en-us/download/details.aspx?id=40855"
}

. .\Scripts\Saritasa.PsakeExtensions.ps1
. .\scripts\Saritasa.PsakeTasks.ps1

. .\scripts\BuildTasks.ps1
. .\scripts\DockerTasks.ps1
. .\scripts\PublishTasks.ps1

Properties `
{
    $SemVer = $env:SemVer
    $MajorMinorPatch = $env:MajorMinorPatch
    $LibDirectory = './build/lib'
    $Configuration = 'Release'

    # For samples.
    $ServerHost = $null
    $SiteName = $null
    $DeployUsername = $null
    $DeployPassword = $null
}

$packages = @(
    'Saritasa.Tools.Common' # common
    'Saritasa.Tools.Domain' # domain
    'Saritasa.Tools.Ef6' # ef6
    'Saritasa.Tools.EfCore1' # efcore1
    'Saritasa.Tools.Emails' # emails
    'Saritasa.Tools.Messages' # messages
    'Saritasa.Tools.Messages.Abstractions' # messages-abstractions
    'Saritasa.Tools.Misc' # misc
    'Saritasa.Tools.Mvc5' # mvc5
    'Saritasa.Tools.NLog4' # nlog4
)

$docsRoot = "$PSScriptRoot\docs"

Task pack -description 'Build the library, test it and prepare nuget packages' `
{
    foreach ($package in $packages)
    {
        &dotnet pack ".\src\$package" --configuration release --output '.'
        if ($LASTEXITCODE)
        {
            throw 'Dotnet pack failed.'
        }
    }
}

Task docs -description 'Compile and open documentation' `
{
    CompileDocs
    Invoke-Item './docs/_build/html/index.html'
}

function CompileDocs
{
    Copy-Item "$docsRoot\conf.py.template" "$docsRoot\conf.py"
    (Get-Content "$docsRoot\conf.py").Replace('VX.VY', $MajorMinorPatch) | Set-Content "$docsRoot\conf.py"

    python -m sphinx.__init__ -b html -d "$docsRoot\_build\doctrees" $docsRoot "$docsRoot\_build\html"
    if ($LASTEXITCODE)
    {
        throw 'Cannot compile documentation.'
    }
}

TaskSetup `
{
    if (!$SemVer)
    {
        # 1.2.3-beta.1
        Expand-PsakeConfiguration @{ SemVer = Exec { GitVersion.exe /showvariable SemVer } }
    }

    if (!$MajorMinorPatch)
    {
        # 1.2.3
        Expand-PsakeConfiguration @{ MajorMinorPatch = Exec { GitVersion.exe /showvariable MajorMinorPatch } }
    }
}
