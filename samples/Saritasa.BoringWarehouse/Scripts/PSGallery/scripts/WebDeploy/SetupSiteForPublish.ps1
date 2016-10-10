
param(
    [parameter(Mandatory = $false)]
    $siteName,

    [parameter(Mandatory = $false)]
    $sitePhysicalPath,

    [parameter(Mandatory = $false)]
    $siteAppPoolName,

    [parameter(Mandatory = $false)]
    [int]$sitePort,

    [parameter(Mandatory = $false)]
    $deploymentUserName,

    [parameter(Mandatory = $false)]
    $deploymentUserPassword,

    [parameter(Mandatory = $false)]
    $managedRunTimeVersion,

    [parameter(Mandatory = $false)]
    $publishSettingSavePath,

    [parameter(Mandatory = $false)]
    $publishSettingFileName
)

# [AZ] 5/2/2016 Embed resources to script.

$Resources = ConvertFrom-StringData @'

Info=Information

Warning=Warning

Error=Error

GrantedPermissions=Granted {0} permissions to {1} on path: {2}

NotGrantedPermissions=Could not grant {0} permissions to {1} on path: {2}

IsAdmin=Confirmed that {0} is a member of the local Administrators group.

CreatedUser=Could not find an existing user account, created local user: {0}

AddedUserAsAdmin=Added {0} to the local Administrators group

CheckIIS7Installed=Failed to load Microsoft.Web.*.dll.  Please verify that IIS 7 is installed.

RuleNotCreated=Skipped creating rule for {0} as it already exists.

CreatedRule=Created delegation rule for provider(s): {0}

NotServerOS=The current SKU is invalid. This script should only be used on a server SKU.

WDeployNotInstalled=Web Deploy must be installed before running this script

HandlerNotInstalled=The IIS 7 Deployment Handler feature of Web Deploy must be installed. Please add this feature in Add/Remove Programs.

SharedConfigInUse=Cannot run this script when Shared Config is enabled.

NoPasswordForGivenUser=Password is required when a user is specified. Please specify password for {0} and try again.

PasswordWillBeReset=No password is specified for {0}. Since ignore reset errors is set, the user password will be reset.

FailedToValidateUserWithSpecifiedPassword=Could not validate user {0} with the specified password.

UpdatedRunAsForSpecificUser=Updated the password for the runAs user {1} specified in the rule for {0}

SiteCreationFailed=Failed to create site. Script will exit now.

FailedToGrantUserAccessToSite=Could not grant IIS Manager permissions for {0} on site {1}.

GrantedUserAccessToSite=Granted IIS Manager permissions for {0} on site {1}.

UserHasAccessToSite=Confirmed that {0} has IIS Manager permissions for site {1}.

FailedToAccessScriptsFolder=Could not access publish settings file save location: {0}. Settings will not be saved.

SavingPublishXmlToPath=Saved publish settings at {0}

FailedToWritePublishSettingsFile=Failed to create publish settings file at {0}.

AppPoolCreated=Created application pool {0}.

AppPoolExists=Application Pool {0} already exists. The application pool may be in use by other applications. It is recommended to have one application pool per site or to disable any appPoolPipeline, appPoolNetFx or recycleApp delegation rules. 

SiteCreated=Created site {0}.

SiteAppPoolUpdated=Confirmed site {0} exists. Application pool for site changed to {1}.

SiteExists=Confirmed site {0} exists and is using application pool {1}.

SiteVirtualDirectoryExists=Skipping site directory creation as directory {0} already exists.  There may be existing content in this directory.

FailedToCreateLogin=Failed to create login {0}

LoginExists=Login {0} already exists.

FailedToCreateDbUser=Failed to create user {0}

DbUserExists=Database User {0} already exists.

FailedToSetupDatabase=Failed to setup database {0}

FailedToCreateDatabase=Failed to create database {0}

DbExists=Database {0} already exists.

SmoNotInstalled=Could not create database. Please make sure that Microsoft SQL Server Management Objects (Version 10 or higher) is installed.

NoPasswordForExistingUserForPublish=Deployment user password was not specified and will not be saved in publish settings file.

CouldNotLoadDeploymentDll=Could not load Microsoft.Web.Deployment.dll.  Please ensure Web Deploy is installed.

ProviderDoesNotExist=The provider {0} is not installed on the system.

CannotSetAndAddProviders=Cannot set both -SetExcludedProviders and -AddExcludedProviders.

ServerBackupConfigChanges=Making server backup configuration changes.

SiteBackupConfigChanges=Making backup configuration changes for site {0}.

BackupSettingEnabled=Setting Enabled to {0}.

BackupSettingPath=Setting Backup Path to: {0}.

BackupSettingPathWarn=Remember to add the proper user permissions to your backup path so that backups can be written there.

BackupSettingNumber=Setting Number of Backups to: {0}.

BackupSettingContinueSync=Setting ContinueSyncOnBackupFailure to {0}.

BackupAddingProviders=Adding provider {0} to ExcludedProviders collection.

BackupSettingCanSetEnabled=Setting CanSetEnabled to {0}.

BackupSettingCanSetNumBackups=Setting CanSetNumBackups to {0}.

BackupSettingCanSetContinueSync=Setting CanSetContinueSyncOnBackupFailure to {0}.

BackupSettingCanSetExcludedProviders=Setting CanSetExcludedProviders to {0}.

BackupResetServerConfig=Resetting server-level backup configuration.

BackupResetSiteConfig=Resetting backup configuration for site {0}.

BackupTurningOn=Turning Backup Feature On.  In order for backups to execute, they still need to be enabled at either the server or site level.

BackupTurningOff=Turning Backup Feature Off.

WritingToLogFile=Writing to log file: {0}.

'@

# ==================================


 #constants
 $SCRIPTERROR = 0
 $WARNING = 1
 $INFO = 2
 $logfile = ".\HostingLog-$(get-date -format MMddyyHHmmss).log"

$template = @"
<?xml version="1.0" encoding="utf-8"?>
<publishData>
  <publishProfile
    publishUrl=""
    msdeploySite=""
    destinationAppUrl=""
    mySQLDBConnectionString=""
    SQLServerDBConnectionString=""
    profileName="Default Settings"
    publishMethod="MSDeploy"
    userName=""
    userPWD=""
    savePWD="True"
    />
</publishData>
"@

#the order is important. Check for apppool name first. Its possible that
#the user gave just a sitename to set permissions. In this case leave apppool emtpy.
#else give a default name to the apppool.
if(!$siteAppPoolName)
{
    if(!$siteName)
    {
        $siteAppPoolName = "WDeployAppPool"
    }
}
else
{
    $siteAppPoolName = $siteAppPoolName.Trim()
}

#now the sitename check. If its empty give it a default name
if(!$siteName)
{
    $siteName = "WDeploySite"
}
else
{
    $siteName = $siteName.Trim()
}

if(!$sitePhysicalPath)
{
    $sitePhysicalPath =  $env:SystemDrive + "\inetpub\WDeploySite"
}
else
{
    $sitePhysicalPath = $sitePhysicalPath.Trim()
}

#global variable. Because we need to return two values from MWA from one function. [REF] has bugs. Hence global
$global:sitePath = $sitePhysicalPath
$global:publishURL = $null

# this function does logging
function write-log([int]$type, [string]$info){

    $message = $info -f $args
    $logMessage = get-date -format HH:mm:ss

    Switch($type)
    {
        $SCRIPTERROR
        {
            $logMessage = $logMessage + "`t" + $Resources.Error + "`t" +  $message
            write-host -foregroundcolor white -backgroundcolor red $logMessage
        }
        $WARNING
        {
            $logMessage = $logMessage + "`t" + $Resources.Warning + "`t" +  $message
            write-host -foregroundcolor black -backgroundcolor yellow $logMessage
        }
        default
        {
            $logMessage = $logMessage + "`t" + $Resources.Info + "`t" +  $message
            write-host -foregroundcolor black -backgroundcolor green  $logMessage
        }
    }

    $logMessage >> $logfile
}


function GetPublishSettingSavePath()
{
    if(!$publishSettingFileName)
    {
        $publishSettingFileName = "WDeploy.PublishSettings"
    }

    if(!$publishSettingSavePath)
    {
        $publishSettingSavePath = [System.Environment]::GetFolderPath("Desktop")
    }

    if((test-path $publishSettingSavePath) -eq $false)
    {
        write-log $SCRIPTERROR $Resources.FailedToAccessScriptsFolder $publishSettingSavePath
        return $null
    }

    return Join-Path $publishSettingSavePath $publishSettingFileName
}

# returns false if OS is not server SKU
function NotServerOS
{
    $sku = $((gwmi win32_operatingsystem).OperatingSystemSKU)
    $server_skus = @(7,8,9,10,12,13,14,15,17,18,19,20,21,22,23,24,25)

    return ($server_skus -notcontains $sku)
}

# gives a user access to an IIS site's scope
function GrantAccessToSiteScope($username, $websiteName)
{
    trap [Exception]
    {
        write-log $SCRIPTERROR $Resources.FailedToGrantUserAccessToSite $username $websiteName
        return $false
    }

    foreach($mInfo in [Microsoft.Web.Management.Server.ManagementAuthorization]::GetAuthorizedUsers($websiteName, $false, 0,[int]::MaxValue))
    {
        if($mInfo.Name -eq $username)
        {
            write-log $INFO $Resources.UserHasAccessToSite $username $websiteName
            return $true
        }
    }

    [Microsoft.Web.Management.Server.ManagementAuthorization]::Grant($username, $websiteName, $FALSE) | out-null
    write-log $INFO $Resources.GrantedUserAccessToSite $username $websiteName
    return $true
}

# gives a user permissions to a file on disk
function GrantPermissionsOnDisk($username, $type, $options)
{
    trap [Exception]
    {
        write-log $SCRIPTERROR $Resources.NotGrantedPermissions $type $username $global:sitePath
    }

    $acl = (Get-Item $global:sitePath).GetAccessControl("Access")
    $accessrule = New-Object system.security.AccessControl.FileSystemAccessRule($username, $type, $options, "None", "Allow")
    $acl.AddAccessRule($accessrule)
    set-acl -aclobject $acl $global:sitePath
    write-log $INFO $Resources.GrantedPermissions $type $username $global:sitePath
}

function AddUser($username, $password)
{
    if(-not (CheckLocalUserExists($username) -eq $true))
    {
        $comp = [adsi] "WinNT://$env:computername,computer"
        $user = $comp.Create("User", $username)
        $user.SetPassword($password)
        $user.SetInfo()
        write-log $INFO $Resources.CreatedUser $username
    }
}

function CheckLocalUserExists($username)
{
    $objComputer = [ADSI]("WinNT://$env:computername")
    $colUsers = ($objComputer.psbase.children | Where-Object {$_.psBase.schemaClassName -eq "User"} | Select-Object -expand Name)

    $blnFound = $colUsers -contains $username

    if ($blnFound)
    {
        return $true
    }
    else
    {
        return $false
    }
}

function CheckIfUserIsAdmin($username)
{
    $computer = [ADSI]("WinNT://$env:computername,computer")
    $group = $computer.psbase.children.find("Administrators")

    $colMembers = $group.psbase.invoke("Members") | %{$_.GetType().InvokeMember("Name",'GetProperty',$null,$_,$null)}

    $bIsMember = $colMembers -contains $username
    if($bIsMember)
    {
        return $true
    }
    else
    {
        return $false
    }
}

function CreateLocalUser($username, $password, $isAdmin)
{
    AddUser $username $password

    if($isAdmin)
    {
        if(-not(CheckIfUserIsAdmin($username) -eq $true))
        {
            $group = [ADSI]"WinNT://$env:computername/Administrators,group"
            $group.add("WinNT://$env:computername/$username")
            write-log $INFO $Resources.AddedUserAsAdmin $username
        }
        else
        {
            write-log $INFO $Resources.IsAdmin $username
        }
    }

    return $true
}

function Initialize
{
    trap [Exception]
    {
        write-log $SCRIPTERROR $Resources.CheckIIS7Installed
        break
    }

    $inetsrvPath = ${env:windir} + "\system32\inetsrv\"

    [System.Reflection.Assembly]::LoadFrom( $inetsrvPath + "Microsoft.Web.Administration.dll" ) > $null
    [System.Reflection.Assembly]::LoadFrom( $inetsrvPath + "Microsoft.Web.Management.dll" )   > $null
}

function GetPublicHostname()
{
    $ipProperties = [System.Net.NetworkInformation.IPGlobalProperties]::GetIPGlobalProperties()
    if($ipProperties.DomainName -eq "")
    {
        return $ipProperties.HostName
    }
    else
    {
        return "{0}.{1}" -f $ipProperties.HostName, $ipProperties.DomainName
    }
}

function GenerateStrongPassword()
{
   [System.Reflection.Assembly]::LoadWithPartialName("System.Web") > $null
   return [System.Web.Security.Membership]::GeneratePassword(12,4)
}

function GetPublishURLFromBindingInfo($bindingInfo, $protocol, $hostname)
{
    $port = 80
    trap [Exception]
    {
        #return defaults
        return "http://$hostname"
    }

    if(($bindingInfo -match "(.*):(\d*):([^:]*)$") -and
        ($Matches.Count -eq 4 ))
    {
        $port = $Matches[2]
        $header = $Matches[3]
        $ipaddress = $Matches[1]
        if($header)
        {
            $hostname = $header
        }
        elseif(($ipaddress) -AND (-not($ipaddress -eq "*")))
        {
            $bracketsArray = @('[',']')
            $hostname  = $ipaddress.Trim($bracketsArray)
        }

        if(-not($port -eq 80))
        {
            $hostname = $hostname + ":" + $port
        }
    }

    return $protocol + "://" + $hostname
}


function GetUnusedPortForSiteBinding()
{
    [int[]] $portArray = $null
    $serverManager = (New-Object Microsoft.Web.Administration.ServerManager)
    foreach($site in $serverManager.Sites)
    {
        foreach($binding in $site.Bindings)
        {
            if($binding.IsIPPortHostBinding)
            {
                if($binding.Protocol -match "https?")
                {
                    if(($binding.BindingInformation -match "(.*):(\d*):([^:]*)$") -and
                    ($Matches.Count -eq 4 ))
                    {
                        $portArray = $portArray + $Matches[2]
                    }
                }
            }
        }
    }

    if(-not($portArray -eq $null))
    {
        $testPortArray = 8080..8200
        foreach($port in $testPortArray)
        {
            if($portArray -notcontains $port)
            {
                return $port
            }
        }
    }

    return 8081 #default
}

function CreateSite($name, $appPoolName, $port, $dotnetVersion)
{
    trap [Exception]
    {
        write-log $SCRIPTERROR $Resources.SiteCreationFailed
        return $false
    }

    $hostname = GetPublicHostName
    $global:publishURL = "http://$hostname"
    if(-not($port -eq 80))
    {
        $global:publishURL = $global:publishURL + ":" + $port
    }

    $configHasChanges = $false
    $serverManager = (New-Object Microsoft.Web.Administration.ServerManager)

    #appPool might be empty. WHen the user gave just a site name to
    #set the permissions on. As long as the sitename is not empty
    if($appPoolName)
    {
        $appPool = $serverManager.ApplicationPools[$appPoolName]
        if ($appPool -eq $null)
        {
            $appPool = $serverManager.ApplicationPools.Add($appPoolName)
            $appPool.Enable32BitAppOnWin64 = $true

            if( ($dotnetVersion) -and
            (CheckVersionWithinAllowedRange $dotnetVersion) )
            {
                $appPool.ManagedRuntimeVersion = $dotnetVersion
            }
            $configHasChanges = $true
            write-log $INFO $Resources.AppPoolCreated $appPoolName
        }
        else
        {
            write-log $WARNING $Resources.AppPoolExists $appPoolName
        }
    }

    $newSite = $serverManager.Sites[$name]
    if ($newSite -eq $null)
    {
        $newSite = $serverManager.Sites.Add($name,$global:sitePath, $port)
        if($appPool)
        {
            $newSite.Applications[0].ApplicationPoolName = $appPool.Name
        }

        if((test-path $global:sitePath) -eq $false)
        {
            [System.IO.Directory]::CreateDirectory($global:sitePath)
        }
        else
        {
            write-log $WARNING $Resources.SiteVirtualDirectoryExists $global:sitePath
        }

        $newSite.ServerAutoStart = $true
        $configHasChanges = $true
        write-log $INFO $Resources.SiteCreated $name
    }
    else
    {
        #get virtual directory and siteport
        $global:sitePath = [System.Environment]::ExpandEnvironmentVariables($newSite.Applications["/"].VirtualDirectories["/"].PhysicalPath)

        foreach($binding in $newSite.Bindings)
        {
            if($binding.IsIPPortHostBinding)
            {
                if($binding.Protocol -match "https?")
                {
                    $global:publishURL = GetPublishURLFromBindingInfo $binding.BindingInformation $binding.Protocol $hostname
                }
            }
        }

        if($appPoolName)
        {
            if (-not($newSite.Applications[0].ApplicationPoolName -eq $appPool.Name ))
            {
                $newSite.Applications[0].ApplicationPoolName = $appPool.Name
                $configHasChanges = $true
                write-log $INFO $Resources.SiteAppPoolUpdated $name $appPoolName
            }
            else
            {
                write-log $INFO $Resources.SiteExists $name $appPoolName
            }
        }
        else
        {
            write-log $INFO $Resources.SiteExists $name $newSite.Applications[0].ApplicationPoolName
        }
    }

    if ($configHasChanges)
    {
        $serverManager.CommitChanges()
    }

    return $true
}

function CheckUserViaLogon($username, $password)
{

 $signature = @'
    [DllImport("advapi32.dll")]
    public static extern int LogonUser(
        string lpszUserName,
        string lpszDomain,
        string lpszPassword,
        int dwLogonType,
        int dwLogonProvider,
        ref IntPtr phToken);
'@

    $type = Add-Type -MemberDefinition $signature  -Name Win32Utils -Namespace LogOnUser  -PassThru

    [IntPtr]$token = [IntPtr]::Zero

    $value = $type::LogOnUser($username, $env:computername, $password, 2, 0, [ref] $token)

    if($value -eq 0)
    {
        return $false
    }

    return $true
 }

function CheckUsernamePasswordCombination($user, $password)
{
    if(($user) -AND ($password))
    {
        if(CheckLocalUserExists($user) -eq $true)
        {
            if(CheckUserViaLogon $user $password)
            {
                return $true
            }
            else
            {
                write-Log $SCRIPTERROR $Resources.FailedToValidateUserWithSpecifiedPassword $user
                return $false
            }
        }
    }

    return $true
}

function CreateProfileXml([string]$nameofSite, [string]$username, $password, [string]$hostname, $pathToSaveFile)
{
    trap [Exception]
    {
        write-log $SCRIPTERROR $Resources.FailedToWritePublishSettingsFile $pathToSaveFile
        return
    }

    $xml = New-Object xml

    if(Test-Path $pathToSaveFile)
    {
        $xml.Load($pathToSaveFile)
    }
    else
    {
        $xml.LoadXml($template)
    }

    $newProfile = (@($xml.publishData.publishProfile)[0])
    $newProfile.publishUrl = $hostname
    $newProfile.msdeploySite = $nameofSite

    $newProfile.destinationAppUrl = $global:publishURL.ToString()
    $newProfile.userName = $username

    if(-not ($password -eq $null))
    {
        $newProfile.userPWD = $password.ToString()
    }
    else
    {
        write-log $WARNING $Resources.NoPasswordForExistingUserForPublish
    }

    $xml.Save($pathToSaveFile)

    write-log $INFO $Resources.SavingPublishXmlToPath $pathToSaveFile
}

function CheckVersionWithinAllowedRange($managedVersion)
{
    trap [Exception]
    {
        return $false
    }

    $KeyPath = "HKLM:\Software\Microsoft\.NETFramework"
    $key = Get-ItemProperty -path $KeyPath
    $path = $key.InstallRoot
    $files = [System.IO.Directory]::GetFiles($path, "mscorlib.dll", [System.IO.SearchOption]::AllDirectories)
    foreach($file in $files)
    {
        if($file -match "\\(v\d\.\d).\d*\\")
        {
            if($Matches[1] -eq $managedVersion)
            {
                return $true
            }
        }
    }
    return $false
}


#================= Main Script =================

if(NotServerOS)
{
    write-log $WARNING $Resources.NotServerOS
    #break [AZ] 4/29/2016 Fix for Windows Server 2012 R2
}

Initialize
if(CheckUsernamePasswordCombination $deploymentUserName $deploymentUserPassword)
{
    if(!$sitePort)
    {
        $sitePort = GetUnusedPortForSiteBinding
    }
    if(CreateSite $siteName $siteAppPoolName $sitePort $managedRunTimeVersion)
    {
        if(!$deploymentUserName)
        {
            $idx = $siteName.IndexOf(' ')
            if( ($idx -gt 0) -or ($siteName.Length -gt 16))
            {
                $deploymentUserName = "WDeployuser"
            }
            else
            {
                $deploymentUserName = $siteName + "user"
            }
        }

        if( (CheckLocalUserExists($deploymentUserName) -eq $true))
        {
            $deploymentUserPassword = $null
        }
        else
        {
            if(!$deploymentUserPassword)
            {
                $deploymentUserPassword = GenerateStrongPassword
            }
        }

        if(CreateLocalUser $deploymentUserName $deploymentUserPassword $false)
        {
            GrantPermissionsOnDisk $deploymentUserName "FullControl" "ContainerInherit,ObjectInherit"

            if(GrantAccessToSiteScope ($env:computername + "\" + $deploymentUserName) $siteName)
            {
                $hostname = GetPublicHostName
                $savePath = GetPublishSettingSavePath
                if($savePath)
                {
                    CreateProfileXml $siteName $deploymentUserName $deploymentUserPassword $hostname $savePath
                }
            }
        }
    }
}
