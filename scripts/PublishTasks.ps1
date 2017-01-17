$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

Import-Module Saritasa.WebDeploy

Task package-zergrushco -depends build-zergrushco `
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
$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

Task package-zerg -depends build-zerg `
{
    $packagePath = "$samples\ZergRushCo.Todosya\Docker\Zerg.zip"
    Invoke-PackageBuild -ProjectPath "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\ZergRushCo.Todosya.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration `
        -BuildParams @("/p:ProjectParametersXMLFile=$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\WebDeployParameters.xml")
}

Task package-bw -depends build-bw `
{
    $packagePath = "$samples\Saritasa.BoringWarehouse\Docker\BW.zip"
    Invoke-PackageBuild -ProjectPath "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.Web\Saritasa.BoringWarehouse.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration -Precompile $false
}
