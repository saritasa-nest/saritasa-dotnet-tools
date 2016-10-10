Add-Type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint srvPoint,
            X509Certificate certificate, WebRequest request,
            int certificateProblem)
        {
            return true;
        }
    }
"@

<#
.SYNOPSIS
Disables SSL check for WebClient requests.
#>
function Update-SslCheckProcedure()
{
    Write-Information 'SSL certificates validation is turned off.'
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
}

<#
.SYNOPSIS
Installs SSL certificate of remote server to trusted certificate root authorities store.

.NOTES
Based on code by Robert Westerlund and Michael J. Lyons.
http://stackoverflow.com/questions/22233702/how-to-download-the-ssl-certificate-from-a-website-using-powershell
#>
function Import-SslCertificate
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ServerHost,
        [int] $Port = 443
    )

    if (!(IsAdmin))
    {
        throw 'Administrator permissions are required.'
    }
    
    $tempFilename = "$env:TEMP\" + [guid]::NewGuid()
    
    $webRequest = [Net.WebRequest]::Create("https://${ServerHost}:$Port")
    try
    {
        $webRequest.GetResponse().Dispose()
    }
    catch [System.Net.WebException]
    {
        if ($_.Exception.Status -EQ [System.Net.WebExceptionStatus]::TrustFailure)
        {
            # Trust failure, do nothing.
        }
        elseif ($_.Exception.Status -EQ [System.Net.WebExceptionStatus]::ProtocolError -And
            $_.Exception.Response.StatusCode -EQ 'NotFound')
        {
            # Page not found, it's OK.
        }
        else
        {
            # Unknown error, rethrow it.
            throw
        }
    }
    
    $cert = $webRequest.ServicePoint.Certificate
    $bytes = $cert.Export([Security.Cryptography.X509Certificates.X509ContentType]::Cert)
    Set-Content -Value $bytes -Encoding Byte -Path $tempFilename

    $cmd = Get-Command Import-Certificate -EA SilentlyContinue
    
    if ($cmd) # Windows 8+
    {
        Import-Certificate -CertStoreLocation Cert:\LocalMachine\Root $tempFilename
    }
    else # Windows 7
    {
        certutil.exe -addstore 'Root' $tempFilename
    }
    
    Remove-Item $tempFilename
}

function IsAdmin
{
    ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')
}
