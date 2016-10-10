$msdeployPath = "$env:ProgramFiles\IIS\Microsoft Web Deploy V3"
$credential = ''

function Set-MsdeployPath
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $script:msdeployPath = $Path
}

<#
.SYNOPSIS

.DESCRIPTION
Leave -Username and -Password empty for NTLM.
For NTLM support execute on server:
Set-ItemProperty HKLM:Software\Microsoft\WebManagement\Server WindowsAuthenticationEnabled 1
Restart-Service WMSVC
https://blogs.msdn.microsoft.com/carlosag/2011/12/13/using-windows-authentication-with-web-deploy-and-wmsvc/
#>
function Set-WebDeployCredential
{
    param
    (
        [System.Management.Automation.PSCredential]
        [System.Management.Automation.Credential()]
        $Credential
    )

    $script:credential = ''
    if ($Credential)
    {
        $username = $Credential.UserName
        $password = $Credential.GetNetworkCredential().Password
        $script:credential = "userName=$username,password=$password,authType=basic"
    }
    else
    {
        $script:credential = "authType='ntlm'"
    }
}

function Assert-WebDeployCredential()
{
    if (!$credential)
    {
        throw 'Credentials are not set.'
    }
}

<#
.EXAMPLE
Invoke-PackageBuild src/WebApp.csproj WebApp.zip -BuildParams ('/p:AspnetMergePath="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools"')
#>
function Invoke-PackageBuild
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ProjectPath,
        [Parameter(Mandatory = $true)]
        [string] $PackagePath,
        [string] $Configuration = 'Release',
        [string] $Platform = 'AnyCPU',
        [bool] $Precompile = $true,
        [string[]] $BuildParams
    )

    $basicBuildParams = ('/m', '/t:Package', "/p:Configuration=$Configuration",
        '/p:IncludeSetAclProviderOnDestination=False', "/p:PrecompileBeforePublish=$Precompile",
        "/p:Platform=$Platform", "/p:PackageLocation=$PackagePath")
    msbuild.exe $ProjectPath $basicBuildParams $BuildParams
    if ($LASTEXITCODE)
    {
        throw 'Package build failed.'
    }
}

<#
.SYNOPSIS

.DESCRIPTION
The recycleApp provider should be delegated to WDeployAdmin.
#>
function Start-AppPool
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $SiteName,
        [Parameter(Mandatory = $true)]
        [string] $Application
    )

    Assert-WebDeployCredential
    'Starting app pool...'
    
    $destArg = "-dest:recycleApp='$SiteName/$Application',recycleMode='StartAppPool'," +
        "computername=https://${ServerHost}:8172/msdeploy.axd?site=$SiteName," + $credential
    $args = @('-verb:sync', '-source:recycleApp', $destArg)
    
    $result = Start-Process -NoNewWindow -Wait -PassThru "$msdeployPath\msdeploy.exe" $args
    if ($result.ExitCode)
    {
        throw 'Msdeploy failed.'
    }
}

<#
.SYNOPSIS

.DESCRIPTION
The recycleApp provider should be delegated to WDeployAdmin.
#>
function Stop-AppPool
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $SiteName,
        [Parameter(Mandatory = $true)]
        [string] $Application
    )

    Assert-WebDeployCredential
    'Stopping app pool...'

    $destArg = "-dest:recycleApp='$SiteName/$Application',recycleMode='StopAppPool'," +
        "computername=https://${ServerHost}:8172/msdeploy.axd?site=$SiteName," + $credential
    $args = @('-verb:sync', '-source:recycleApp', $destArg)
    
    $result = Start-Process -NoNewWindow -Wait -PassThru "$msdeployPath\msdeploy.exe" $args
    if ($result.ExitCode)
    {
        throw 'Msdeploy failed.'
    }
}

<#
.SYNOPSIS

.DESCRIPTION
The recycleApp provider should be delegated to WDeployConfigWriter.
#>
function Invoke-WebDeployment
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $PackagePath,
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $SiteName,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Application
    )

    Assert-WebDeployCredential
    "Deploying $PackagePath to $ServerHost/$Application..."
    
    "https://${ServerHost}:8172/msdeploy.axd"
    
    $destArg = 
    $args = @("-source:package='$PackagePath'",
              ("-dest:auto,computerName='https://${ServerHost}:8172/msdeploy.axd?site=$SiteName',includeAcls='False'," + $credential),
              '-verb:sync', '-disableLink:AppPoolExtension', '-disableLink:ContentExtension', '-disableLink:CertificateExtension',
              '-allowUntrusted', "-setParam:name='IIS Web Application Name',value='$SiteName/$Application")
    
    $result = Start-Process -NoNewWindow -Wait -PassThru "$msdeployPath\msdeploy.exe" $args 
    if ($result.ExitCode)
    {
        throw 'Msdeploy failed.'
    }
}

<#
.SYNOPSIS
Copies IIS app content from local server to remote server.
#>
function Sync-IisApp
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $SiteName,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Application,
        [Parameter(Mandatory = $true)]
        [string] $DestinationServer
    )

    Assert-WebDeployCredential
    $args = @('-verb:sync', "-source:iisApp='$SiteName/FormI9Verify'",
              ("-dest:auto,computerName='https://${DestinationServer}:8172/msdeploy.axd?site=$SiteName'," + $credential))

    $result = Start-Process -NoNewWindow -Wait -PassThru "$msdeployPath\msdeploy.exe" $args 
    if ($result.ExitCode)
    {
        throw 'Msdeploy failed.'
    }
    
    Write-Information "Updated '$SiteName/$Application' app on $DestinationServer server."
}

<#
.SYNOPSIS
Synchronizes web site file structure between local and remote servers.
#>
function Sync-WebContent
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ContentPath,
        [Parameter(Mandatory = $true)]
        [string] $DestinationServer,
        [Parameter(Mandatory = $true)]
        [string] $SiteName
    )

    Assert-WebDeployCredential
    $args = @('-verb:sync', "-source:contentPath='$ContentPath'",
              ("-dest:auto,computerName='https://${DestinationServer}:8172/msdeploy.axd?site=$SiteName'," + $credential))

    $result = Start-Process -NoNewWindow -Wait -PassThru "$msdeployPath\msdeploy.exe" $args 
    if ($result.ExitCode)
    {
        throw 'Msdeploy failed.'
    }
    
    Write-Information "Updated '$ContentPath' directory on $DestinationServer server."
}
