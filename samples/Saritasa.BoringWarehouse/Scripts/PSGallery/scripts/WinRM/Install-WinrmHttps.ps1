<#PSScriptInfo

.VERSION 1.4.2

.GUID 3ccd77cd-d928-4e72-98fc-82e3417f3427

.AUTHOR Anton Zimin

.COMPANYNAME Saritasa

.COPYRIGHT (c) 2016 Saritasa. All rights reserved.

.TAGS WinRM

.LICENSEURI https://raw.githubusercontent.com/dermeister0/PSGallery/master/LICENSE

.PROJECTURI https://github.com/dermeister0/PSGallery

.ICONURI 

.EXTERNALMODULEDEPENDENCIES 

.REQUIREDSCRIPTS 

.EXTERNALSCRIPTDEPENDENCIES 

.RELEASENOTES

#>

<#
.SYNOPSIS
Configures server to accept WinRM connections over HTTPS.

.DESCRIPTION
Generates self-signed certificate or uses existing. Configures HTTPS listener for WinRM service. Opens 5986 port in firewall.

For Windows Server 2008 you should execute following statement to disable remote UAC:
Set-ItemProperty –Path HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System –Name LocalAccountTokenFilterPolicy –Value 1 –Type DWord
#> 
param
(
    [string] $CertificateThumbprint,
    [switch] $Force
)

trap
{
    Write-Error 'FAILURE'
    $_
    $host.SetShouldExit(1)
    exit
}

$hostname = [System.Net.Dns]::GetHostByName('localhost').Hostname

if (!$CertificateThumbprint)
{
    $existingCertificate = Get-ChildItem -Path Cert:\LocalMachine\My |
        Where-Object { $_.Subject -EQ "CN=$hostname" } | Select-Object -First 1
    if ($existingCertificate)
    {
        $CertificateThumbprint = $existingCertificate.Thumbprint
        Write-Information 'Using existing certificate...'
    }
    else
    {
        $CertificateThumbprint = (New-SelfSignedCertificate -DnsName $hostname -CertStoreLocation Cert:\LocalMachine\My).Thumbprint
        Write-Information 'New certificate is generated.'
    }
}

$existingListener = Get-ChildItem WSMan:\localhost\Listener |
    Where-Object { $_.Keys[0] -eq 'Transport=HTTPS' }

if ($existingListener)
{
    Write-Information 'Listener already exists.'
    if ($Force)
    {
        Write-Information 'Reinstalling...'
        Remove-Item "WSMan:\localhost\Listener\$($existingListener.Name)" -Recurse
        $existingListener = $null
    }
}

if (!$existingListener)
{
    New-Item -Path WSMan:\localhost\Listener -Address * -Transport HTTPS -Hostname $hostname `
        -CertificateThumbprint $CertificateThumbprint -Force
    Write-Information 'New listener is created.'
}

try
{
    New-NetFirewallRule -DisplayName 'Windows Remote Management (HTTPS-In)' `
        -Direction Inbound -Action Allow -Protocol TCP -LocalPort 5986 -ErrorAction Stop
    Write-Information 'Firewall rule is updated.'
}
catch [Microsoft.Management.Infrastructure.CimException]
{
    if ($_.Exception.HResult -eq 0x80131500)
    {
        Write-Information 'Windows Firewall is not enabled.'
    }
    else
    {
        throw
    }
}

Write-Information "`nWinRM is set up for host $hostname."
