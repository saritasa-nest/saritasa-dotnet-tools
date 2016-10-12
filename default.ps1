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

Properties `
{
    $SignKey = './saritasa-tools.snk'
    $NuspecFile = './build/Saritasa.Tools.nuspec'
    $LibDirectory = './build/lib'
    $Configuration = 'Release'
}

# Global variable.
$script:Version = '0.0.0'

$builds = @(
    @{Id = 'v4.5.2'; Framework = 'net452'; symbol = 'NET452'}
    @{Id = 'v4.6.1'; Framework = 'net461'; symbol = 'NET461'}
)
$packages = @(
    'Saritasa.Tools'
    'Saritasa.Tools.Ef6'
    'Saritasa.Tools.Mvc5'
    'Saritasa.Tools.NLog4'
)

$docsRoot = Resolve-Path "$PSScriptRoot\docs"

function Get-PackageName([string]$package)
{
    If ([System.Char]::IsDigit($package[$package.Length-1])) {$package.Substring(0, $package.Length-1)} Else {$package}
}

Task pack -depends pre-build, get-version -description 'Build the library, test it and prepare nuget packages' `
{
    # build all versions, sign, test and prepare package directory
    foreach ($package in $packages)
    {
        Get-PackageName($package)
        Remove-Item $LibDirectory -Recurse -Force -ErrorAction SilentlyContinue
        New-Item $LibDirectory -ItemType Directory
        foreach ($build in $builds)
        {
            # build & sign
            $id = $build.Id
            $symbol = $build.symbol
            $args = @("/p:TargetFrameworkVersion=$id", "/p:DefineConstants=$symbol")
            if (Test-Path $SignKey)
            {
                $args += "/p:AssemblyOriginatorKeyFile=$SignKey" + '/p:SignAssembly=true'
            }
            Invoke-ProjectBuild -ProjectPath "./src/$package/$package.csproj" -Configuration $Configuration -BuildParams $args

            # copy
            New-Item (Join-Path $LibDirectory $build.Framework) -ItemType Directory
            $packageFile = Get-PackageName $package
            Copy-Item "./src/$package/bin/$Configuration/$packageFile.dll" (Join-Path $LibDirectory $build.Framework)
            Copy-Item "./src/$package/bin/$Configuration/$packageFile.XML" (Join-Path $LibDirectory $build.Framework)
        }

        # pack, we already have nuget in current folder
        $nugetExePath = "$PSScriptRoot\tools\nuget.exe"
        $buildDirectory = (Get-Item $LibDirectory).Parent.FullName
        &"$nugetExePath" @('pack', (Join-Path $buildDirectory "$package.nuspec"), '-Version', $Version, '-NonInteractive', '-Exclude', '*.snk')
        if ($LASTEXITCODE)
        {
            throw 'Nuget pack failed.'
        }
    }

    # little clean up
    Remove-Item $LibDirectory -Recurse -Force -ErrorAction SilentlyContinue

    # build docs
    CompileDocs
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
