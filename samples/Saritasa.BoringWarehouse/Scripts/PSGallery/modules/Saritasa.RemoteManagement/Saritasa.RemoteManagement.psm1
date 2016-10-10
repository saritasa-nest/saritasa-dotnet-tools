$credential = $null

function Set-RemoteManagementCredential
{
    param
    (
        [System.Management.Automation.PSCredential]
        [System.Management.Automation.Credential()]
        $Credential
    )

    $script:credential = $Credential
}

function ExecuteAppCmd
{
    param
    (
        [string] $ServerHost,
        [string] $ConfigFilename,
        [string[]] $Arguments
    )

    $config = Get-Content $ConfigFilename
    $appCmd = "$env:SystemRoot\System32\inetsrv\appcmd"
    
    if ($ServerHost) # Remote server.
    {
        if (!$credential)
        {
            throw 'Credentials are not set.'
        }
        
        $session = Start-RemoteSession $ServerHost

        Invoke-Command -Session $session -ScriptBlock { $using:config | &$using:appCmd $using:Arguments }

        Remove-PSSession $session
    }
    else # Local server.
    {
        Invoke-Command { $config | &$appCmd $Arguments }
    }
    
    if ($LASTEXITCODE)
    {
        throw 'AppCmd failed.'
    }
}

function GetAppCmdOutput
{
    param
    (
        [string] $ServerHost,
        [string[]] $Arguments
    )

    $appCmd = "$env:SystemRoot\System32\inetsrv\appcmd"
    
    if ($ServerHost) # Remote server.
    {
        if (!$credential)
        {
            throw 'Credentials are not set.'
        }
        
        $session = Start-RemoteSession $ServerHost

        $output = Invoke-Command -Session $session -ScriptBlock { &$using:appCmd $using:Arguments }

        Remove-PSSession $session
    }
    else # Local server.
    {
        $output = Invoke-Command { &$appCmd $Arguments }
    }
    
    if ($LASTEXITCODE)
    {
        throw 'AppCmd failed.'
    }
    
    $output | Where-Object { $_.Length -ne 0 }
}

function Import-AppPool
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $ConfigFilename
    )

    ExecuteAppCmd $ServerHost $ConfigFilename @('add', 'apppool', '/in') $false
    'App pools are updated.'
}

function Import-Site
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $ConfigFilename
    )

    ExecuteAppCmd $ServerHost $ConfigFilename @('add', 'site', '/in') $false
    'Web sites are updated.'
}

function CreateOutputDirectory([string] $Filename)
{
    $dir = Split-Path $Filename
    if (!(Test-Path $dir))
    {
        New-Item $dir -ItemType directory
    }
}

function Export-AppPool
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $OutputFilename
    )
    
    CreateOutputDirectory $OutputFilename
    $xml = GetAppCmdOutput $ServerHost @('list', 'apppool', '/config', '/xml')
    $xml | Set-Content $OutputFilename
}

function Export-Site
{
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Hostname of the server with IIS site configured.')]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $OutputFilename
    )

    CreateOutputDirectory $OutputFilename
    $xml = GetAppCmdOutput $ServerHost @('list', 'site', '/config', '/xml')
    $xml | Set-Content $OutputFilename
}

function Start-RemoteSession
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost
    )
    
    New-PSSession -UseSSL -Credential $credential -ComputerName ([System.Net.Dns]::GetHostByName($ServerHost).Hostname)
}

function Install-Iis
{
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseSingularNouns", "",
                                                       Scope="Function", Target="*")]

    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [switch] $ManagementService,
        [switch] $WebDeploy,
        [switch] $UrlRewrite
    )
    
    $session = Start-RemoteSession $ServerHost
    
    Invoke-Command -Session $session -ScriptBlock { Add-WindowsFeature Web-Server, Web-Asp-Net45 }
    Write-Information 'IIS is set up successfully.'

    if ($ManagementService)
    {
        Install-WebManagementService -Session $session
    }
    
    if ($WebDeploy)
    {
        Install-WebDeploy -Session $session
    }
    
    if ($UrlRewrite)
    {
        Install-UrlRewrite -Session $session
    }
    
    Invoke-Command -Session $session -ScriptBlock `
        {
            if (Get-WebSite -Name 'Default Web Site')
            {
                Remove-WebSite -Name 'Default Web Site'
                Get-ChildItem C:\inetpub\wwwroot -Recurse | Remove-Item -Recurse
                Write-Information 'Default Web Site is deleted.'
            }
        }

    Remove-PSSession $session
}

function CheckSession
{
    param
    (
        [string] $ServerHost,
        [System.Management.Automation.Runspaces.PSSession] $Session
    )
    
    if (!$Session)
    {
        if (!$ServerHost)
        {
            throw 'ServerHost is not set.'
        }
        $Session = Start-RemoteSession $ServerHost
    }
    
    $Session
}

function Install-WebManagementService
{
    param
    (
        [string] $ServerHost,
        [System.Management.Automation.Runspaces.PSSession] $Session
    )

    $Session = CheckSession $ServerHost $Session
    
    Invoke-Command -Session $session -ScriptBlock `
        {
            # Install web management service.
            Add-WindowsFeature Web-Mgmt-Service
            # Enable remote access.
            Set-ItemProperty -Path HKLM:\SOFTWARE\Microsoft\WebManagement\Server -Name EnableRemoteManagement -Value 1
            # Change service startup type to automatic.
            Set-Service WMSVC -StartupType Automatic
            
            # Replace WMSvc-HOST with HOST certificate. It should be generated already during WinRM configuration.
            Import-Module WebAdministration
            $hostname = [System.Net.Dns]::GetHostByName('localhost').Hostname
            $thumbprint = Get-ChildItem -Path Cert:\LocalMachine\My |
                Where-Object { $_.Subject -EQ "CN=$hostname" } |
                Select-Object -First 1 -ExpandProperty Thumbprint
            if (!$thumbprint)
            {
                "SSL certificate for $hostname host is not found."
            }            
            if (Test-Path IIS:\SslBindings\0.0.0.0!8172)
            {
                Remove-Item -Path IIS:\SslBindings\0.0.0.0!8172
            }
            Get-Item -Path "Cert:\LocalMachine\My\$thumbprint" | New-Item -Path IIS:\SslBindings\0.0.0.0!8172

            # Start web management service.
            Start-Service WMSVC
        }

    Write-Information 'Web management service is installed and configured.'
}

function Install-WebDeploy
{
    param
    (
        [string] $ServerHost,
        [System.Management.Automation.Runspaces.PSSession] $Session
    )
    
    $Session = CheckSession $ServerHost $Session
    
    Invoke-Command -Session $session -ScriptBlock `
        {
            # 1.1 = {0F37D969-1260-419E-B308-EF7D29ABDE20}
            # 2.0 = {5134B35A-B559-4762-94A4-FD4918977953}
            # 3.5 = {3674F088-9B90-473A-AAC3-20A00D8D810C}
            $webDeploy36Guid = '{ED4CC1E5-043E-4157-8452-B5E533FE2BA1}'
            $installedProduct = Get-CimInstance -Class Win32_Product -Filter "IdentifyingNumber = '$webDeploy36Guid'"
            
            if ($installedProduct)
            {
                'WebDeploy is installed already.'
            }
            else
            {       
                $webDeploy36Url = 'https://download.microsoft.com/download/0/1/D/01DC28EA-638C-4A22-A57B-4CEF97755C6C/WebDeploy_amd64_en-US.msi'
                $tempPath = "$env:TEMP\" + [guid]::NewGuid()
                
                'Downloading WebDeploy installer...'
                Invoke-WebRequest $webDeploy36Url -OutFile $tempPath -ErrorAction Stop
                'OK'
                
                msiexec.exe /i $tempPath ADDLOCAL=MSDeployFeature,MSDeployUIFeature,DelegationUIFeature,MSDeployWMSVCHandlerFeature | Out-Null
                if ($LASTEXITCODE)
                {
                    throw 'MsiExec failed.'
                }
        
                Remove-Item $tempPath -ErrorAction SilentlyContinue
                'WebDeploy is installed.'
            }
        }
}

<#
.SYNOPSIS
Executes a script on a remote server.

.NOTES
Based on code by mjolinor.
http://stackoverflow.com/a/27799658/991267
#>
function Invoke-RemoteScript
{
    param
    (
        [string] $Path,
        [hashtable] $Parameters,
        [string] $ServerHost,
        [System.Management.Automation.Runspaces.PSSession] $Session
    )
    
    $Session = CheckSession $ServerHost $Session
    
    $scriptContent = Get-Content $Path -Raw
    $scriptParams = &{$args} @Parameters
    $sb = [scriptblock]::create("&{ $scriptContent } $scriptParams")

    Invoke-Command -Session $Session -ScriptBlock $sb
}

function Install-UrlRewrite
{
    param
    (
        [string] $ServerHost,
        [System.Management.Automation.Runspaces.PSSession] $Session
    )
    
    $Session = CheckSession $ServerHost $Session

    Invoke-Command -Session $session -ScriptBlock `
        {
            $urlRewrite20Guid = '{08F0318A-D113-4CF0-993E-50F191D397AD}'
            $installedProduct = Get-CimInstance -Class Win32_Product -Filter "IdentifyingNumber = '$urlRewrite20Guid'"
            
            if ($installedProduct)
            {
                'URL Rewrite Module is installed already.'
            }
            else
            {       
                $urlRewrite20Url = 'http://download.microsoft.com/download/C/9/E/C9E8180D-4E51-40A6-A9BF-776990D8BCA9/rewrite_amd64.msi'
                $tempPath = "$env:TEMP\" + [guid]::NewGuid()
                
                'Downloading URL Rewrite Module installer...'
                Invoke-WebRequest $urlRewrite20Url -OutFile $tempPath -ErrorAction Stop
                'OK'
                
                msiexec.exe /i $tempPath ADDLOCAL=ALL | Out-Null
                if ($LASTEXITCODE)
                {
                    throw 'MsiExec failed.'
                }
        
                Remove-Item $tempPath
                'URL Rewrite Module is installed.'
            }
        }
}

<#
.NOTES
Msiexec supports HTTP links.
#>
function Install-MsiPackage
{
    param
    (
        [string] $ServerHost,
        [System.Management.Automation.Runspaces.PSSession] $Session,
        [Parameter(Mandatory = $true)]
        [string] $ProductName,
        [Parameter(Mandatory = $true)]
        [string] $ProductId,
        [Parameter(Mandatory = $true)]
        [string] $MsiPath,
        [string] $LocalFeatures = 'ALL'     
    )
    
    $Session = CheckSession $ServerHost $Session
    
    Invoke-Command -Session $session -ScriptBlock `
        {
            $installedProduct = Get-CimInstance -Class Win32_Product -Filter "IdentifyingNumber = '$using:ProductId'"
            
            if ($installedProduct)
            {
                Write-Information "$using:ProductName is installed already."
            }
            else
            {       
                msiexec.exe /i $using:MsiPath ADDLOCAL=$using:LocalFeatures | Out-Null
                if ($LASTEXITCODE)
                {
                    throw 'MsiExec failed.'
                }
                
                Write-Information "$using:ProductName is installed."
            }
        }
}

<#
.SYNOPSIS
Creates a new directory in remote server's %TEMP% and returns it.
#>
function Get-RemoteTempPath
{
    param
    (
        [Parameter(Mandatory = $true)]
        [System.Management.Automation.Runspaces.PSSession] $Session
    )

    Invoke-Command -Session $Session -ScriptBlock `
        {
            $tempPath = "$env:TEMP\" + [guid]::NewGuid()
            New-Item $tempPath -ItemType directory -ErrorAction Stop | Out-Null
            $tempPath
        }
}

function Import-SslCertificate
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [Parameter(Mandatory = $true)]
        [string] $CertificatePath,
        [Parameter(Mandatory = $true)]
        [System.Security.SecureString] $CertificatePassword
    )
    
    $Session = Start-RemoteSession $ServerHost
    
    $name = (Get-Item $CertificatePath).Name
    $tempPath = Get-RemoteTempPath $Session
    Copy-Item -Path $CertificatePath -Destination $tempPath -ToSession $Session
    
    Invoke-Command -Session $Session -ScriptBlock `
        {
            Import-PfxCertificate "$using:tempPath\$using:name" -CertStoreLocation 'Cert:\LocalMachine\My' -Password $using:CertificatePassword
            
            Remove-Item $using:tempPath -Recurse -Force
        }
        
    Remove-PSSession $Session
}
