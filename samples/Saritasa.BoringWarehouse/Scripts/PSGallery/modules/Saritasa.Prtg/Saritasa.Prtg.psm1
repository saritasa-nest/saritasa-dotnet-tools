$prtgUrl  = ''
$userName = ''
$password = ''
$sensors = @{}

<#
.SYNOPSIS

.EXAMPLE
Initialize-Prtg prtg.local admin Qwerty123 @{ Server1 = 10123; Server2 = 10124; }
#>
function Initialize-Prtg
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $PrtgUrl,
        [Parameter(Mandatory = $true)]
        [System.Management.Automation.PSCredential]
        [System.Management.Automation.Credential()]
        $Credential,
        [Parameter(Mandatory = $true)]
        [hashtable] $Sensors
    )
    
    $script:prtgUrl = $PrtgUrl
    $script:userName = $Credential.UserName
    $script:password = $Credential.GetNetworkCredential().Password
    $script:sensors = $Sensors
}

function Get-PrtgSensorId
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Server
    )

    $sensorId = $sensors[$Server]
    if (!$sensorId)
    {
        throw "PRTG sensor for $Server is not registered."
    }
    
    $sensorId
}

function Start-PrtgSensor
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Server
    )

    'Starting PRTG sensor...'
    $sensorId = Get-PrtgSensorId($Server)
    Update-SslCheckProcedure
    $status = (Invoke-WebRequest "$prtgUrl/api/pause.htm?id=$sensorId&action=1&pausemsg=Resumed by deployment script.&username=$username&password=$password").StatusDescription
    "$status`n`n"
}

function Stop-PrtgSensor
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Server
    )

    'Stopping PRTG sensor...'
    $sensorId = Get-PrtgSensorId($Server)
    Update-SslCheckProcedure
    $status = (Invoke-WebRequest "$prtgUrl/api/pause.htm?id=$sensorId&action=0&pausemsg=Paused by deployment script.&username=$username&password=$password").StatusDescription
    "$status`n`n"
}
