# Requires psake to run, see README.md for more details.

Framework 4.6
$InformationPreference = 'Continue'
$env:PSModulePath += ";$PSScriptRoot\Scripts\Modules"

if ($PSVersionTable.PSVersion.Major -lt 3)
{
    throw "PowerShell 3 is required.`nhttp://www.microsoft.com/en-us/download/details.aspx?id=40855"
}

. .\scripts\Saritasa.PsakeTasks.ps1

. .\scripts\BuildTasks.ps1
. .\scripts\DockerTasks.ps1
. .\scripts\PublishTasks.ps1

Properties `
{
    $Version = '0.1.0'
    $LibDirectory = './build/lib'
    $Configuration = 'Release'
}

# Global variables.
$script:Version = '0.0.0'

$packages = @(
    'Saritasa.Tools.Common'# common
    'Saritasa.Tools.Domain' # domain
    'Saritasa.Tools.EF6' # ef6
    'Saritasa.Tools.EFCore1' # efcore1
    'Saritasa.Tools.Emails' # emails
    'Saritasa.Tools.Messages' # messages
    'Saritasa.Tools.Messages.Abstractions' # messages-abstractions
    'Saritasa.Tools.Misc' # misc
    'Saritasa.Tools.Mvc5' # mvc5
    'Saritasa.Tools.NLog4' # nlog4
)

$docsRoot = Resolve-Path "$PSScriptRoot\docs"

Task pack -depends download-nuget -description 'Build the library, test it and prepare nuget packages' `
{
    # Build all versions, sign, test and prepare package directory.
    foreach ($package in $packages)
    {
        &dotnet build ".\src\$package" --configuration release

        # Prepare library folder.
        Remove-Item $LibDirectory -Recurse -Force -ErrorAction SilentlyContinue
        New-Item $LibDirectory -ItemType Directory
        foreach ($build in Get-ChildItem ".\src\$package\bin\Release" -Recurse | ?{ $_.PSIsContainer })
        {
            Copy-Item -Path ".\src\$package\bin\Release\$build" -Destination $LibDirectory `
                -Exclude '*.pdb' -Recurse -Container -Force
        }

        # Pack, we already have nuget in current folder.
        $nugetExePath = "$PSScriptRoot\tools\nuget.exe"
        &"$nugetExePath" @('pack', ".\src\$package\$package.nuspec", `
            '-BasePath', "$LibDirectory\..", `
            '-NonInteractive', `
            '-Exclude', '*.snk')
        if ($LASTEXITCODE)
        {
            throw 'Nuget pack failed.'
        }
    }

    # Little clean up.
    Remove-Item $LibDirectory -Recurse -Force -ErrorAction SilentlyContinue
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
