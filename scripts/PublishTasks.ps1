$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

Task package-zerg -depends build-zerg `
{
    $packagePath = "$samples\ZergRushCo.Todosya\Docker\Zerg.zip"
    Invoke-PackageBuild -ProjectPath "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\ZergRushCo.Todosya.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration
}

Task publish-bw -depends build-bw `
{
    $packagePath = "$samples\Saritasa.BoringWarehouse\Docker\BW.zip"
    Invoke-PackageBuild -ProjectPath "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.Web\Saritasa.BoringWarehouse.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration -Precompile $false
}
