Properties `
{
    $ServerHost = $null
    $SiteName = $null
    $DeployUsername = $null
    $DeployPassword = $null
}

$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"
$workspace = Resolve-Path "$root\.."

Task pre-publish -description 'Set common publish settings for all deployments.' `
{
    $credential = $null # Use NTLM authentication.

    if ($DeployUsername)
    {
        $credential = New-Object System.Management.Automation.PSCredential($DeployUsername, (ConvertTo-SecureString $DeployPassword -AsPlainText -Force))
    }
    Initialize-WebDeploy -Credential $credential
}

Task package-zergrushco -depends build-zergrushco `
{
    $packagePath = "$workspace\ZergRushCo.zip"
    Invoke-PackageBuild -ProjectPath "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\ZergRushCo.Todosya.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration `
        -BuildParams @("/p:ProjectParametersXMLFile=$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\WebDeployParameters.xml")
}

Task package-boringwarehouse -depends pre-build `
{
    $packagePath = "$workspace\BoringWarehouse.zip"
    Invoke-PackageBuild -ProjectPath "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.Web\Saritasa.BoringWarehouse.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration
}

Task publish-zergrushco -depends pre-publish `
    -requiredVariables @('ServerHost', 'SiteName') `
{
    Invoke-WebDeployment -PackagePath "$workspace\ZergRushCo.zip" -ServerHost $ServerHost `
        -SiteName $SiteName -Application '' `
        -MSDeployParams @("-setParam:name='AppDbContext-Web.config Connection String',value='Data Source=zergdb;Initial Catalog=ZergRushCo;User ID=TestUser;Password=gHJT2SCm'",
            "-setParam:name='AppDbContextProviderName',value='System.Data.SqlClient'")
}

Task publish-boringwarehouse -depends pre-publish `
    -requiredVariables @('ServerHost', 'SiteName') `
{
    Invoke-WebDeployment -PackagePath "$workspace\BoringWarehouse.zip" -ServerHost $ServerHost `
        -SiteName $SiteName -Application '' `
        -MSDeployParams @("-setParam:name='AppDbContext-Web.config Connection String',value='Data Source=bwdb;Initial Catalog=BoringWarehouse;User ID=TestUser;Password=gHJT2SCm'")
}
