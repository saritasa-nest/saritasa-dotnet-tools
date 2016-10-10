$apiKey = ''

function Initialize-NewRelic
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ApiKey
    )

    $script:ApiKey = $ApiKey
}

function Update-NewRelicDeployment
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ApplicationId,
        [Parameter(Mandatory = $true)]
        [string] $Revision
    )

    Invoke-RestMethod -uri https://api.newrelic.com/deployments.xml -Method Post `
        -Headers @{ 'x-api-key' = $apiKey } `
        -Body @{ 'deployment[application_id]' = $ApplicationId; 'deployment[revision]' = $Revision }
}
