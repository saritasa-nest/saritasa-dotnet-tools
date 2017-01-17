$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

Import-Module Saritasa.WebDeploy

Task package-zergrushco -depends build-zergrushco `
{
    $packagePath = "$samples\ZergRushCo.Todosya\Docker\ZergRushCo.zip"
    Invoke-PackageBuild -ProjectPath "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\ZergRushCo.Todosya.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration `
        -BuildParams @("/p:ProjectParametersXMLFile=$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\WebDeployParameters.xml")
}

Task package-boringwarehouse -depends pre-build `
{
    $packagePath = "$samples\Saritasa.BoringWarehouse\Docker\BoringWarehouse.zip"
    Invoke-PackageBuild -ProjectPath "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.Web\Saritasa.BoringWarehouse.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration
}
