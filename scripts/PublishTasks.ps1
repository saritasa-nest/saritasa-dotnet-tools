$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

Import-Module "$root\Saritasa.WebDeploy.psd1"

Task package-zerg -depends build-zerg `
{
    Invoke-PackageBuild -ProjectPath "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\ZergRushCo.Todosya.Web.csproj" `
        -PackagePath 'Zerg.zip' -Configuration $Configuration -Precompile $false
}
