# Requires psake to run, see README.md for more details.

Framework 4.7
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
    $Configuration = 'Release'
}

# Global variables.
$script:Version = '0.0.0'

$packages = @(
    'Saritasa.Tools.Common'# common
    'Saritasa.Tools.Domain' # domain
    'Saritasa.Tools.EF6' # ef6
    'Saritasa.Tools.EFCore3' # efcore3
    'Saritasa.Tools.Emails' # emails
    'Saritasa.Tools.Messages' # messages
    'Saritasa.Tools.Messages.Abstractions' # messages-abstractions
    'Saritasa.Tools.Messages.TestRuns' # testruns
    'Saritasa.Tools.Misc' # misc
)

$docsRoot = Resolve-Path "$PSScriptRoot\docs"

Task pack -depends download-nuget, clean -description `
    'Build the library, test it and prepare nuget packages' `
{
    $revcount = &'git' @('rev-list', '--all', '--count') | Out-String | ForEach-Object { $_ -replace [Environment]::NewLine, '' }
    $hash = &'git' @('log', '--pretty=format:%h', '-n', '1') | Out-String | ForEach-Object { $_ -replace [Environment]::NewLine, '' }
    $longHash = &'git' @('rev-parse', 'HEAD') | Out-String | ForEach-Object { $_ -replace [Environment]::NewLine, '' }
    foreach ($package in $packages)
    {
        # Format versions.
        $assemblyVersion = GetPackageAssemblyVersion($package)
        $fileVersion = (Get-Content ".\src\$package\VERSION.txt").Trim() + ".$revcount"
        $productVersion = "$fileVersion-$hash"
        Write-Information "$package has versions $assemblyVersion $fileVersion $productVersion"

        # Update version for every assembly.
        ReplaceVersionInAssemblyInfo ".\src\$package\Properties\AssemblyInfo.cs" 'AssemblyVersion' $assemblyVersion
        ReplaceVersionInAssemblyInfo ".\src\$package\Properties\AssemblyInfo.cs" 'AssemblyFileVersion' $fileVersion
        ReplaceVersionInAssemblyInfo ".\src\$package\Properties\AssemblyInfo.cs" 'AssemblyInformationalVersion' $productVersion
    }

    foreach ($package in $packages)
    {
        # Build.
        &dotnet restore ".\src\$package"
        &dotnet build ".\src\$package" --configuration release

        # Prepare nuspec file.
        Copy-Item ".\src\$package\$package.nuspec.template" ".\src\$package\$package.nuspec" -Force
        Update-VariablesInFile -Path ".\src\$package\$package.nuspec" `
            -Variables @{ CommitHash = $longHash }

        # Pack.
        $assemblyVersion = GetVersion($package)
        &"$PSScriptRoot\tools\nuget.exe" @('pack', ".\src\$package\$package.nuspec", `
            '-NonInteractive', `
            '-Version', $assemblyVersion,
            '-Exclude', '*.snk')
        if ($LASTEXITCODE)
        {
            throw 'Nuget pack failed.'
        }
    }

    foreach ($package in $packages)
    {
        # Revert versions changes.
        ReplaceVersionInAssemblyInfo ".\src\$package\Properties\AssemblyInfo.cs" 'AssemblyVersion' '1.0.0.0'
        ReplaceVersionInAssemblyInfo ".\src\$package\Properties\AssemblyInfo.cs" 'AssemblyFileVersion' '1.0.0.0'
        ReplaceVersionInAssemblyInfo ".\src\$package\Properties\AssemblyInfo.cs" 'AssemblyInformationalVersion' '1.0.0.0'
    }
}

Task clean -description 'Clean solution' `
{
    Remove-Item './Saritasa.*.nupkg' -ErrorAction SilentlyContinue
    Remove-Item './Saritasa.*.zip' -ErrorAction SilentlyContinue
    Remove-Item './src/*.suo' -Recurse -Force -ErrorAction SilentlyContinue
    foreach ($package in $packages)
    {
        Remove-Item "./src/$package/bin/*" -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item "./src/$package/obj/*" -Recurse -Force -ErrorAction SilentlyContinue
    }
    Remove-Item "./test/Saritasa.Tools.Common.Tests/bin/*" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "./test/Saritasa.Tools.Common.Tests/obj/*" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "./test/Saritasa.Tools.Messages.Benchmark/bin/*" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "./test/Saritasa.Tools.Messages.Benchmark/obj/*" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "./test/Saritasa.Tools.Messages.Tests/bin/*" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "./test/Saritasa.Tools.Messages.Tests/obj/*" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "./test/Saritasa.Tools.Tests/bin/*" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "./test/Saritasa.Tools.Tests/obj/*" -Recurse -Force -ErrorAction SilentlyContinue
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

function GetVersion($package)
{
    return (Get-Content ".\src\$package\VERSION.txt").Trim()
}

function GetPackageAssemblyVersion($package)
{
    $version = GetVersion($package)
    return $version.Substring(0, $version.LastIndexOf('.')) + '.0.0'
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

function ReplaceVersionInAssemblyInfo($file, $attribute, $version)
{
    $pattern = "$attribute\(""[0-9]+(\.([0-9a-zA-Z\-]+|\*)){1,3}""\)"
    $version = "$attribute(""$version"")"

    (Get-Content $file) | ForEach-Object `
        {
            ForEach-Object { $_ -replace $pattern, $version }
        } | Set-Content $file -Encoding UTF8
}
