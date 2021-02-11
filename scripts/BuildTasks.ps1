$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"
$tools = "$root\..\tools"

Task download-nuget `
{
    if (!(Test-Path $tools))
    {
        New-Item -ItemType Directory $tools
    }

    Install-NugetCli -Destination $tools
}

Task get-version `
{
    $script:Version = GitVersion.exe /showvariable MajorMinorPatch
}
