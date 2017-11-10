Properties `
{
    $Configuration = $null
    $SemVer = $null
}

$root = $PSScriptRoot
$workspace = Resolve-Path "$root\.."

Task docker-zergrushco -depends package-zergrushco `
    -requiredVariables @('SemVer') `
{
    $dockerContext = "$samples\ZergRushCo.Todosya\Docker"

    Exec { docker build -t zerg/web:latest -t "zerg/web:$SemVer" -f "$dockerContext\Dockerfile.web" $workspace }
    Exec { docker build -t zerg/db:latest -t "zerg/db:$SemVer" -f "$dockerContext\Dockerfile.db" $dockerContext }
}

Task docker-boringwarehouse -depends package-boringwarehouse `
    -requiredVariables @('SemVer') `
{
    $dockerContext = "$samples\Saritasa.BoringWarehouse\Docker"

    Exec { docker build -t bw/web:latest -t "bw/web:$SemVer" -f "$dockerContext\Dockerfile.web" $workspace }
    Exec { docker build -t bw/db:latest -t "bw/db:$SemVer" -f "$dockerContext\Dockerfile.db" $dockerContext }
}

# Docker images should be built before run (docker-boringwarehouse task).
Task run-boringwarehouse-tests `
    -requiredVariables @('Configuration') `
{
    # Recreate and containers.
    Exec { docker-compose -f "$samples\Saritasa.BoringWarehouse\docker-compose.yml" up --force-recreate -d db }

    # Get IP address of DB container.
    $ipAddress = Exec { docker inspect saritasaboringwarehouse_db_1 -f '{{ .NetworkSettings.Networks.nat.IPAddress }}' }
    Write-Information "DB container address: $ipAddress"

    # Wait for SQL Server to start.
    $retries = 0
    while ($true)
    {
        $result = Test-NetConnection -ComputerName $ipAddress -Port 1433
        if ($result.TcpTestSucceeded)
        {
            break
        }
        else
        {
            $retries++
            if ($retries -eq 10)
            {
                throw 'Test database is not available.'
            }
            else
            {
                Start-Sleep $retries
            }
        }
    }

    # Replace connection string.
    $appConfigPath = "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.IntegrationTests\bin\$Configuration\Saritasa.BoringWarehouse.IntegrationTests.dll.config"
    $lines = Get-Content $appConfigPath
    $lines | % `
        {
            $_ -replace '(^\s*)<add name="AppDbContext" connectionString="(.*)" .*$',
                "$1<add name=`"AppDbContext`" connectionString=`"Data Source=$ipAddress;Initial Catalog=BoringWarehouse;User ID=TestUser;Password=gHJT2SCm`" providerName=`"System.Data.SqlClient`" />"
        } | Set-Content $appConfigPath
    Write-Information "Updated $appConfigPath."

    Invoke-NUnit3Runner "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.IntegrationTests\bin\$Configuration\Saritasa.BoringWarehouse.IntegrationTests.dll"
}

Task docker-run-all -depends docker-boringwarehouse, docker-zergrushco `
{
    Exec { docker-compose -f "$samples\Saritasa.BoringWarehouse\docker-compose.yml" up -d }
    Exec { docker-compose -f "$samples\ZergRushCo.Todosya\docker-compose.yml" up -d }

    $ipAddress = Exec { docker inspect saritasaboringwarehouse_web_1 -f '{{ .NetworkSettings.Networks.nat.IPAddress }}' }
    Write-Information "Saritasa.BoringWarehouse: http://$ipAddress"

    $ipAddress = Exec { docker inspect zergrushcotodosya_web_1 -f '{{ .NetworkSettings.Networks.nat.IPAddress }}' }
    Write-Information "ZergRushCo.Todosya: http://$ipAddress"
}
