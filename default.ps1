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
    $Configuration = 'Release'

    # For samples.
    $ServerHost = $null
    $SiteName = $null
    $DeployUsername = $null
    $DeployPassword = $null
}

$packages = @(
    'Saritasa.Tools.Common'# common
    'Saritasa.Tools.Domain' # domain
    'Saritasa.Tools.EF6' # ef6
    'Saritasa.Tools.EFCore1' # efcore1
    'Saritasa.Tools.Emails' # emails
    'Saritasa.Tools.Messages' # messages
    'Saritasa.Tools.Messages.Abstractions' # messages-abstractions
    'Saritasa.Tools.Messages.TestRuns' # testruns
    'Saritasa.Tools.Misc' # misc
)

$docsRoot = "$PSScriptRoot\docs"

Task pack -description 'Build the library, test it and prepare nuget packages' `
{
    $revcount = &'git' @('rev-list', '--all', '--count') | Out-String | ForEach-Object { $_ -replace [Environment]::NewLine, '' }
    $hash = &'git' @('log', '--pretty=format:%h', '-n', '1') | Out-String | ForEach-Object { $_ -replace [Environment]::NewLine, '' }
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

        # Pack.
        $assemblyVersion = GetVersion($package)
        &"$PSScriptRoot\tools\nuget.exe" @('pack', ".\src\$package\$package.nuspec", `
            '-NonInteractive', `
            '-Version', $assemblyVersion,
            '-Exclude', '*.snk')
        if ($LASTEXITCODE)
        {
            throw 'Dotnet pack failed.'
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

Task docs -description 'Compile and open documentation' `
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
    (Get-Content "$docsRoot\conf.py").Replace('VX.VY', $MajorMinorPatch) | Set-Content "$docsRoot\conf.py"

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
