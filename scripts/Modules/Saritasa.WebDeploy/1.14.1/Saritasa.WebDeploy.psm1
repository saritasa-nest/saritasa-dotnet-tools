$msdeployPath = "$env:ProgramFiles\IIS\Microsoft Web Deploy V3"
$msdeployPort = 8172
$credential = ''

<#
.SYNOPSIS

.DESCRIPTION
Leave -Username and -Password empty for NTLM.
For NTLM support execute on server:
Set-ItemProperty HKLM:Software\Microsoft\WebManagement\Server WindowsAuthenticationEnabled 1
Restart-Service WMSVC
https://blogs.msdn.microsoft.com/carlosag/2011/12/13/using-windows-authentication-with-web-deploy-and-wmsvc/
#>
function Initialize-WebDeploy
{
    [CmdletBinding()]
    param
    (
        [System.Management.Automation.PSCredential]
        [System.Management.Automation.Credential()]
        $Credential,
        [string] $MsdeployPath,
        [int] $MsdeployPort
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

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

    if ($MsdeployPath)
    {
        $script:msdeployPath = $MsdeployPath
    }

    if ($MsdeployPort)
    {
        $script:msdeployPort = $MsdeployPort
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
    [CmdletBinding()]
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

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

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
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $SiteName,
        [Parameter(Mandatory = $true)]
        [string] $Application
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    Assert-WebDeployCredential
    Write-Information 'Starting app pool...'

    $computerName, $useTempAgent = GetComputerName $ServerHost $SiteName
    $destArg = "-dest:recycleApp='$SiteName/$Application',recycleMode='StartAppPool'," +
        "computerName='$computerName',tempAgent='$useTempAgent'," + $credential
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
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $SiteName,
        [Parameter(Mandatory = $true)]
        [string] $Application
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    Assert-WebDeployCredential
    Write-Information 'Stopping app pool...'

    $computerName, $useTempAgent = GetComputerName $ServerHost $SiteName
    $destArg = "-dest:recycleApp='$SiteName/$Application',recycleMode='StopAppPool'," +
        "computerName='$computerName',tempAgent='$useTempAgent'," + $credential
    $args = @('-verb:sync', '-source:recycleApp', $destArg)

    $result = Start-Process -NoNewWindow -Wait -PassThru "$msdeployPath\msdeploy.exe" $args
    if ($result.ExitCode)
    {
        throw 'Msdeploy failed.'
    }
}

<#
.OUTPUTS
computerName, useTempAgent
#>
function GetComputerName([string] $ServerHost, [string] $SiteName)
{
    $computerName = $null
    $useAgent = $false
    $useTempAgent = $false

    if (Test-IsLocalhost $ServerHost) # Local server.
    {
        # Check if Web Deployment Agent Service exists.
        $agentService = Get-Service MsDepSvc -ErrorAction SilentlyContinue
        if ($agentService)
        {
            $useAgent = $true
        }
        elseif (Test-Path "$env:ProgramFiles\IIS\Microsoft Web Deploy\MsDepSvc.exe")
        {
            # Temp agent is available.
            $useAgent = $true
            $useTempAgent = $true
        }
    }

    if ($useAgent) # Local server, Agent Service available.
    {
        $computerName = 'localhost'
    }
    else # Remote server, use Web Management Service.
    {
        $computerName = "https://${ServerHost}:$msdeployPort/msdeploy.axd?site=$SiteName"
    }

    $computerName
    $useTempAgent
}

<#
.SYNOPSIS

.DESCRIPTION
The recycleApp provider should be delegated to WDeployConfigWriter.
#>
function Invoke-WebDeployment
{
    [CmdletBinding()]
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
        [string] $Application,
        [switch] $AllowUntrusted,
        [string[]] $MSDeployParams
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    Assert-WebDeployCredential
    Write-Information "Deploying $PackagePath to $ServerHost/$Application..."

    $computerName, $useTempAgent = GetComputerName $ServerHost $SiteName
    $args = @("-source:package='$PackagePath'",
              ("-dest:auto,computerName='$computerName',tempAgent='$useTempAgent',includeAcls='False'," + $credential),
              '-verb:sync', '-disableLink:AppPoolExtension', '-disableLink:ContentExtension', '-disableLink:CertificateExtension',
              "-setParam:name='IIS Web Application Name',value='$SiteName/$Application'")

    if ($AllowUntrusted)
    {
        $args += '-allowUntrusted'
    }

    if ($MSDeployParams)
    {
        $args += $MSDeployParams
    }

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
    [CmdletBinding()]
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

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    Assert-WebDeployCredential

    $computerName, $useTempAgent = GetComputerName $DestinationServer $SiteName
    $args = @('-verb:sync', "-source:iisApp='$SiteName/$Application'",
              ("-dest:auto,computerName='$computerName',tempAgent='$useTempAgent'," + $credential))

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
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ContentPath,
        [Parameter(Mandatory = $true)]
        [string] $DestinationServer,
        [Parameter(Mandatory = $true)]
        [string] $SiteName,
        [Parameter(Mandatory = $true, ParameterSetName = 'IIS')]
        [AllowEmptyString()]
        [string] $Application,
        [Parameter(Mandatory = $true, ParameterSetName = 'FileSystem')]
        [switch] $AutoDestination
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    Assert-WebDeployCredential

    $destinationParam = $null
    if ($AutoDestination)
    {
        $destinationParam = '-dest:auto'
    }
    else
    {
        $destinationParam = "-dest:iisApp='$SiteName/$Application'"
    }

    $computerName, $useTempAgent = GetComputerName $DestinationServer $SiteName
    $args = @('-verb:sync', "-source:contentPath='$ContentPath'",
              ("$destinationParam,computerName='$computerName',tempAgent='$useTempAgent'," + $credential))

    $result = Start-Process -NoNewWindow -Wait -PassThru "$msdeployPath\msdeploy.exe" $args
    if ($result.ExitCode)
    {
        throw 'Msdeploy failed.'
    }

    Write-Information "Updated '$ContentPath' directory on $DestinationServer server."
}

<#
.SYNOPSIS
Deploys ASP.NET web site (app without project) to remote server. It's similar to Sync-WebContent, but creates IIS application.
#>
function Invoke-WebSiteDeployment
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Path,
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $SiteName,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Application,
        [switch] $AllowUntrusted,
        [string[]] $MSDeployParams
    )

    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState

    Assert-WebDeployCredential
    Write-Information "Deploying web site from $Path to $ServerHost/$Application..."

    $computerName, $useTempAgent = GetComputerName $ServerHost $SiteName
    $args = @('-verb:sync', "-source:iisApp='$Path'",
              ("-dest:iisApp='$SiteName/$Application',computerName='$computerName',tempAgent='$useTempAgent'," + $credential))

    if ($AllowUntrusted)
    {
        $args += '-allowUntrusted'
    }

    if ($MSDeployParams)
    {
        $args += $MSDeployParams
    }

    $result = Start-Process -NoNewWindow -Wait -PassThru "$msdeployPath\msdeploy.exe" $args
    if ($result.ExitCode)
    {
        throw 'Msdeploy failed.'
    }
}
