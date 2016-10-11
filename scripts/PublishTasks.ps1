$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

Import-Module Saritasa.WebDeploy

Task package-zergrushco -depends pre-build `
{
    $packagePath = "$samples\ZergRushCo.Todosya\ZergRushCo.zip"
    Invoke-PackageBuild -ProjectPath "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\ZergRushCo.Todosya.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration
}

Task package-boringwarehouse -depends pre-build `
{
    $packagePath = "$samples\Saritasa.BoringWarehouse\BoringWarehouse.zip"
    Invoke-PackageBuild -ProjectPath "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.Web\Saritasa.BoringWarehouse.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration
}
