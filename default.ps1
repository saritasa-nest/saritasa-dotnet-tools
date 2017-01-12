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
. .\scripts\PublishTasks.ps1

Properties `
{
    $version = '0.1.0'
    $signKey = './saritasa-tools.snk'
    $nuspecFile = './build/Saritasa.Tools.nuspec'
    $libDirectory = './build/lib'
    $Configuration = 'Release'
}

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

function GetPackageName([string]$package)
{
    If ([System.Char]::IsDigit($package[$package.Length-1])) {$package.Substring(0, $package.Length-1)} Else {$package}
}

Task pack -depends download-nuget -description 'Build the library, test it and prepare nuget packages' `
{
    # nuget restore
    Invoke-NugetRestore './src/Saritasa.Tools.sln'

    # update version
    Update-AssemblyInfoFile $version

    # build all versions, sign, test and prepare package directory
    foreach ($package in $packages)
    {
        $packageFile = GetPackageName $package
        Write-Information $packageFile

        Remove-Item $libDirectory -Recurse -Force -ErrorAction SilentlyContinue
        New-Item $libDirectory -ItemType Directory
        foreach ($build in $builds)
        {
            # build & sign
            $id = $build.Id
            $symbol = $build.symbol
            $args = @("/p:TargetFrameworkVersion=$id", "/p:DefineConstants=$symbol")
            if (Test-Path $signKey)
            {
                $args += "/p:AssemblyOriginatorKeyFile=$signKey" + '/p:SignAssembly=true'
            }
            Invoke-ProjectBuild -ProjectPath "./src/$package/$package.csproj" -Configuration 'Release' -BuildParams $args

            # copy
            New-Item (Join-Path $libDirectory $build.Framework) -ItemType Directory
            Copy-Item "./src/$package/bin/Release/$packageFile.dll" (Join-Path $libDirectory $build.Framework)
            Copy-Item "./src/$package/bin/Release/$packageFile.XML" (Join-Path $libDirectory $build.Framework)
        }

        # pack, we already have nuget in current folder
        $nugetExePath = "$PSScriptRoot\scripts\nuget.exe"
        $buildDirectory = (Get-Item $libDirectory).Parent.FullName
        Exec { &$nugetExePath @('pack', (Join-Path $buildDirectory "$package.nuspec"), '-Version', $version, '-NonInteractive', '-Exclude', '*.snk') }
    }

    # little clean up
    Remove-Item $libDirectory -Recurse -Force -ErrorAction SilentlyContinue

    # build docs
    CompileDocs
}

Task clean -description 'Clean solution' `
{
    Remove-Item $libDirectory -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item './Saritasa.*.nupkg' -ErrorAction SilentlyContinue
    Remove-Item './src/*.suo' -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item './src/Saritasa.Tools/bin' -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item './src/Saritasa.Tools/obj' -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item './src/StyleCop.Cache' -Force -ErrorAction SilentlyContinue
    Remove-Item './docs/_build' -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item './docs/conf.py' -ErrorAction SilentlyContinue
    Remove-Item './scripts/nuget.exe' -ErrorAction SilentlyContinue
}

Task docs -description 'Compile and open documentation' `
{
    CompileDocs
    Invoke-Item './docs/_build/html/index.html'
}

function CompileDocs
{
    Set-Location '.\docs'
    Copy-Item '.\conf.py.template' '.\conf.py'
    (Get-Content '.\conf.py').replace('VX.VY', $version) | Set-Content '.\conf.py'
    Exec { &'.\make.cmd' @('html') }
    Set-Location '..'
}
