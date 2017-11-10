$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

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

function DeployLocalProject([string] $packagePath, [string] $connectionString, [string[]] $params)
{
    $allParams = @('-verb:sync',
        "-source:package='$packagePath'",
        '-dest:auto',
        "-setParam:name='IIS Web Application Name',value='Default Web Site/'",
        "-setParam:name='AppDbContext-Web.config Connection String',value='$connectionString'")

    if ($params)
    {
        $allParams += $params
    }

    Start-Process -Wait -NoNewWindow -PassThru 'C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe' $allParams
}

Task publish-zergrushco `
{
    DeployLocalProject 'ZergRushCo.zip' 'Data Source=zergdb;Initial Catalog=ZergRushCo;User ID=TestUser;Password=gHJT2SCm' `
        "-setParam:name='AppDbContextProviderName',value='System.Data.SqlClient'"
}

Task publish-boringwarehouse `
{
    DeployLocalProject 'BoringWarehouse.zip' 'Data Source=bwdb;Initial Catalog=BoringWarehouse;User ID=TestUser;Password=gHJT2SCm'
}
